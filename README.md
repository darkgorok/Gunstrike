# Gunstrike

Unity action-platformer/shooter project based on the `_SuperCommando_` gameplay package, now with a staged DI/service migration using VContainer.

## Current Architecture

The project now has a root composition layer built on VContainer:

- `Assets/_SuperCommando_/Script/System/Bootstrap`
- `Assets/_SuperCommando_/Script/System/Contracts`
- `Assets/_SuperCommando_/Script/System/Loading`
- `Assets/_SuperCommando_/Script/System/Services/Legacy`
- `Assets/_SuperCommando_/Script/GUI/Loading`

### Composition Root

- `ProjectLifetimeScope` is the root `LifetimeScope`
- `ProjectLifetimeScopeBootstrap` ensures the root scope exists before scene code asks for services
- `ProjectScope.Inject(this)` is the compatibility entry point for scene `MonoBehaviour` classes

### Registered Services

Current service contracts registered in `ProjectLifetimeScope`:

- `ISceneLoader`
- `IAudioService`
- `IAdsService`
- `IGameplayPresentationService`
- `ILevelCatalogService`
- `IPlayerProfileService`
- `IProgressService`
- `IInventoryService`
- `IUpgradeService`
- `ILevelSelectionState`
- `ICharacterSelectionService`
- `IGameSessionService`
- `IControllerInputService`
- `IKeyValueStore`
- `ICameraRigService`
- `IMenuFlowService`
- `IKeyPresentationService`
- `IDialogFlowService`
- `IMainMenuSceneService`
- `ITutorialOverlayService`
- `IFloatingTextService`
- `IBossHealthbarService`
- `IGunRuntimeService`
- `IKeyboardBindingService`
- `ILevelMapSettingsService`
- `IDefaultGameConfigService`
- `IAnalyticsService`
- `IConsentService`

These are currently backed by legacy adapters where needed, so the migration is incremental rather than a risky rewrite.

## Loading Flow

The old timer-based loading implementation was replaced with a proper scene-loading architecture:

- `LoadingScreenView` is now a dumb view component
- `SceneLoaderService` owns scene transition orchestration
- scene loading uses real `AsyncOperation` progress
- loading UI and loading orchestration are separated
- common scene transitions are centralized through `ISceneLoader`

The old `TimedLoadingScreen` was removed and replaced by `LoadingScreenView`.

## Refactor Slices Already Migrated

### Scene Navigation / Loading

Main scene transition flow was moved away from local duplicated coroutines and into `ISceneLoader`:

- `MenuManager`
- `MainMenuHomeScene`
- `FlashScene`
- `StoryComic`
- `ResetData`
- `OpenScene`
- `GameManager`

### Menu / Progress / Upgrade UI

UI scripts that used to reach directly into `GlobalValue` now go through service contracts:

- `MainMenu_Level`
- `MainMenu_World`
- `WorldChooseUI`
- `MainMenuUpdateCoins`
- `UpgradeItemUI`
- `Menu_GUI`
- `Menu_StartScreen`
- `CharacterHolder`
- `MapControllerUI`

### Inventory / Weapon Flow

Weapon and ammo related flow now uses `IInventoryService` and `IUpgradeService`:

- `GunManager`
- `RangeAttack`
- `MeleeAttack`
- `Player`
- `CollectGunItem`
- `ItemType`
- `ShopItemUI`

Recent follow-up work in this slice:

- `LegacyInventoryService` now owns current selected gun runtime state instead of routing that state back through `GlobalValue`
- dart ammo persistence goes through `IKeyValueStore`
- upgrade values already route through `IUpgradeService`
- `LegacyPlayerProfileService` now uses explicit key constants instead of depending on `GlobalValue` string fields
- `LegacyLevelSelectionState` now owns level-selection runtime state directly instead of proxying through `GlobalValue.currentHighestLevelObj`
- `GlobalValue` and `GunTypeID` now route persistence through `IKeyValueStore` fallback stores instead of touching `PlayerPrefs` inline
- `Player` is now physically split into `Player.cs`, `Player.Runtime.cs`, and `Player.Combat.cs` partials so movement/runtime/combat code no longer lives in one monolithic source file

### Camera / Menu / HUD Bridging

The project now also exposes explicit bridges for camera and menu-driven runtime orchestration:

- `ICameraRigService`
- `IMenuFlowService`
- `IKeyPresentationService`

These services now front legacy scene objects instead of letting gameplay code talk directly to them:

- `GameManager`
- `Player`
- `ControllerInput`
- `CameraLookPoint`
- `DialogUITrigger`
- `FloatingTextManager`
- `FloatingText`
- `Menu_AskSaveMe`
- `WatchAdToFinishLevel`

This removed the remaining direct gameplay dependencies on `CameraFollow.Instance`, `MenuManager.Instance`, and `KeyUI.Instance` from the core runtime flow.

### Secondary Legacy Bridges

Another adapter layer now covers secondary scene-level singleton flows that were still hanging off template-era `Instance` access:

- `IDialogFlowService`
- `IMainMenuSceneService`
- `ITutorialOverlayService`
- `IFloatingTextService`
- `IBossHealthbarService`
- `IGunRuntimeService`
- `IKeyboardBindingService`
- `ILevelMapSettingsService`
- `IDefaultGameConfigService`

These are now used to reduce direct singleton reach-ins from:

- `DialogHandler`
- `DialogUITrigger`
- `OpenScene`
- `MainMenu_Level`
- `WorldChooseUI`
- `TutorialFlag`
- `TheGate`
- `BOSS_1`
- `CollectGunItem`
- `Player`
- `RangeAttack`
- `ControllerInput`
- `Menu_AskSaveMe`
- `MainMenuHomeScene`
- `LevelManager`
- `LegacyProgressService`
- `GlobalValue`

As a result, the old `DialogManager.Instance`, `MainMenuHomeScene.Instance`, `Tutorial.Instance`, `BossHealthbar.Instance`, `FloatingTextManager.Instance`, `DefaultValueKeyboard.Instance`, `LevelMapType.Instance`, and `GunManager.Instance` access patterns are no longer used by gameplay callers.

### Ads / Rewarded Flow

Rewarded and interstitial entry points are abstracted through `IAdsService`:

- `Menu_AskSaveMe`
- `WatchAdToFinishLevel`
- `MainMenu_Level`
- `MainMenuHomeScene`
- `MenuManager`
- `ShopItemUI`
- `FlashScene`

This also removed reliance on older broken rewarded-ads call sites in UI code.

The ad orchestration layer itself is also partially modernized:

- `AdsManager` now uses explicit polling/backoff timers instead of `StartCoroutine` / `Invoke`
- ad removal checks go through progress state rather than direct `GlobalValue.RemoveAds` reads
- `LegacyAdsService` still provides compatibility for the rest of the project

### Store Readiness / Consent

The app now has a dedicated launch-consent and analytics layer:

- `IConsentService`
- `IAnalyticsService`
- `ConsentService`
- `ReflectionFirebaseAnalyticsService`
- `ConsentDialogController`

What this adds:

- a branded consent dialog is shown immediately on launch before the logo scene continues
- the consent dialog is now expected to be authored in the Unity editor instead of being generated at runtime
- tapping `ACCEPT` persists consent locally
- tapping `ACCEPT` triggers the analytics event `consent_accepted`
- if Firebase Analytics is present in the project, the event is forwarded automatically through the reflection-based adapter
- the app also syncs Google UMP consent state after acceptance when running on Android/iOS

Related startup polish included in this slice:

- `FlashScene` is now gated by launch consent instead of always auto-advancing after a fixed delay
- `MainMenuHomeScene` no longer overrides menu music with gameplay music during startup
- the `Ads Manager` prefab instance in `Logo Scene` is active again, fixing a broken startup path where ad services never initialized

### Gameplay Presentation

Legacy singleton-based presentation logic was abstracted behind `IGameplayPresentationService`:

- black-screen transitions
- controller visibility
- gameplay UI visibility
- warning / clean encounter UI

Used by:

- `BOSS_FLYING_BOMBER`
- `BOSS_HOGRIDER`
- `BOSS_1`
- `DialogUITrigger`
- `DialogManager`
- `CameraLookPoint`
- `GroupEnemySystem`
- `StoryComic`

### Runtime Session / Game Flow

Runtime game-session access is now abstracted behind `IGameSessionService`, backed by `LegacyGameSessionService` over `GameManager`.

The runtime session layer now also has dedicated infrastructure for internal core state:

- `IGameSessionRuntimeState`
- `GameSessionRuntimeState`
- `IListenerBroadcastService`
- `SceneListenerBroadcastService`

This migration removed direct orchestration dependencies from a number of gameplay scripts:

- `BOSS_FLYING_BOMBER`
- `BOSS_HOGRIDER`
- `BOSS_1`
- `BOSS_2`
- `BOSS_3`
- `GroupEnemySystem`
- `KeyItem`
- `TheGate`
- `GameFinishFlag`
- `Menu_Gameover`
- `KillPlayerOnTouch`
- `Menu_AskSaveMe`
- `MonsterIV`
- `MonsterFish`
- `EnemyTank`
- `Turret`

The service currently covers:

- game state
- player access
- key possession
- checkpoint access
- mission stars
- session points/coins
- continue cost
- camera pause
- game over / continue / finish / restart orchestration

The underlying `GameManager` runtime flow is also cleaner than the original template version:

- finish / continue transitions now use explicit timer-driven state changes instead of `Invoke` / coroutines
- debug/reset shortcuts now route through service abstractions instead of writing directly to `PlayerPrefs` or `GlobalValue`
- listener notification is now delegated to a dedicated broadcast service instead of being owned inline by `GameManager`
- transient runtime values such as state, checkpoint, mission stars, point/coin totals and several flags now live in `GameSessionRuntimeState` instead of being owned only as mutable fields on `GameManager`
- the session lifecycle is now more predictable for `LegacyGameSessionService`

Additional runtime callers migrated onto `IGameSessionService` in later passes include:

- `AutoSpawn`
- `AutoSetBoolAnim`
- `BrokenTreasure`
- `HelicopterController`
- `KeyUI`
- `Star`
- `EnemyHideAndShow`
- `EnemyHitAndHide`
- `Spring`
- `LaserObstacle`
- `Teleport`
- `SetActiveObjInRange`
- `PlaySoundWhenPlayerInRange`
- `PlaySoundWhenObjMovingHelper`
- `StandFallingSpikedObject`
- `GiveDamageToTarget`
- `ChasingStone`
- `FallingProjectileBulletBullet`
- `PathedProjectileSpawner`
- `Grenade`
- `SetLayerFollowPlayer`
- `SpearTrap`
- `TruckleController`
- `TruckleTrigger`
- `SimplePathedMoving`
- `Tutorial`
- `TeleportPoint`
- `SimpleChasePlayer`
- `Boss_SuperAttackFlame`
- `PressureSwitchEvent`
- `EffectChooseHelper`

Editor/debug tooling and store integration were also cleaned up in later passes:

- `GameModeEditor` now uses `IKeyValueStore` semantics through `UnityPrefsKeyValueStore` instead of writing `PlayerPrefs`/`GlobalValue` inline
- `Purchaser` now routes `RemoveAds` through `IProgressService` instead of directly mutating `GlobalValue`
- `UnityIAPNotifyDelayer` now uses `EditorPrefs` instead of `PlayerPrefs`
- `LegacyGameSessionService` and `LegacyControllerInputService` no longer depend on `GameManager.Instance` / `ControllerInput.Instance`, and instead resolve scene objects lazily without singleton coupling
- `LegacyAudioService` no longer uses `SoundManager` as a static gateway; it now adapts a concrete `SoundManager` instance
- `PlayerPrefs` storage is now named and registered as `UnityPrefsKeyValueStore`, keeping persistence concerns isolated behind `IKeyValueStore`

### Enemy Runtime / State Machines

Shared enemy runtime infrastructure has started moving away from ad-hoc coroutines and duplicated timing logic:

- `IEnemyState`
- `EnemyStateMachine`
- `BossDeathSequence`
- `BurstFireController`

These are now used to remove coroutine-based orchestration from:

- `BOSS_FLYING_BOMBER`
- `BOSS_HOGRIDER`
- `BOSS_1`
- `BOSS_2`
- `BOSS_3`
- `EnemyRangeAttack`
- `EnemyTank`
- `Turret`
- `EnemyAI`
- `Enemy`
- `EnemyGrounded`
- `SimpleFlyingEnemy`
- `MonsterSnail`
- `SmartEnemyGrounded`
- `EnemyMeleeAttack`
- `EnemyThrowAttack`
- `EnemyBomberAttack`
- `EnemyCallMinion`

This gives the project a reusable path for moving enemy logic to explicit update-driven state machines instead of hidden coroutine flows.

Smaller legacy bosses are also moving onto explicit timer-driven orchestration:

- `BOSS_1`
- `BOSS_2`
- `BOSS_3`
- `Boss1AttackOrder`

In that slice:

- attack loops no longer self-schedule through coroutines
- blink/hurt/death cleanup no longer relies on `Invoke`
- orientation and combat checks use injected runtime services instead of direct singleton calls
- boss attack ordering is now an explicit update-driven loop instead of a hidden coroutine chain

### Projectile / Combat Utilities

Projectile-side combat helpers have also started moving onto the same service-oriented runtime assumptions:

- `ArrowProjectile`
- `SimpleProjectile`
- `SmartProjectileAttack`
- `PathedProjectile`
- `MonsterFish`
- `FishAI`
- `FireEvil`

In this slice:

- projectile hit audio now goes through `IAudioService`
- score rewards now go through `IGameSessionService`
- local delayed destruction / reload / attack delays use explicit timers instead of coroutine helpers

### Environment / Trap Runtime

Another batch of scene interactables and traps now uses injected runtime services instead of direct singleton access:

- `Spring`
- `LaserObstacle`
- `Teleport`
- `SetActiveObjInRange`
- `PlaySoundWhenPlayerInRange`
- `PlaySoundWhenObjMovingHelper`
- `StandFallingSpikedObject`
- `GiveDamageToTarget`
- `ChasingStone`
- `FallingProjectileBulletBullet`
- `PathedProjectileSpawner`
- `Grenade`
- `SetLayerFollowPlayer`
- `SpearTrap`
- `TruckleController`
- `TruckleTrigger`
- `SimplePathedMoving`
- `Tutorial`
- `TeleportPoint`
- `SimpleChasePlayer`
- `EnemyJumping`
- `EnemySpider`
- `MagnetItem`
- `Block`
- `MonsterUpDown`
- `PlatformControllerSwitcher`
- `Miner`
- `SimpleGravityObject`
- `Bridge`
- `FallingProjectileBullet`

In this slice:

- player access, game-state checks and camera pause go through `IGameSessionService`
- local trigger/repeat flows are moving away from `InvokeRepeating` and coroutine-driven throttling toward explicit timers
- trap and obstacle audio now routes through `IAudioService` where the script itself owns the playback decision
- several late legacy gameplay components now inject `IAudioService` directly instead of calling `SoundManager.PlaySfx(...)`

### Component Lookup Cleanup

A dedicated cleanup pass also normalized Unity component lookup patterns in frequently-hit scripts:

- same-object dependencies now prefer cached references with `SerializeField` + `Awake/Start` fallback
- repeated lookup paths were removed from `Bridge`, `GameFinishFlag`, `MainMenu_Level`, `MainMenu_World`, `ZoomZone`, `CameraFollow`, `ControllerInput`, `GameManager`, `LevelManager`, `GroupEnemySystem`, `MainMenuHomeScene`, `ResetData`, `HealthBar`, and `ChasingStone`
- `FindObjectOfType` usage was reduced to a small residual tail, with most scene lookups moved to cached `Object.FindFirstObjectByType(...)` or explicit serialized references
- required same-object dependencies now use `RequireComponent` where appropriate

## Folder Conventions

Use these rules for new code:

- put contracts in `Assets/_SuperCommando_/Script/System/Contracts`
- put root bootstrap/container code in `Assets/_SuperCommando_/Script/System/Bootstrap`
- put scene-loading orchestration in `Assets/_SuperCommando_/Script/System/Loading`
- put compatibility adapters in `Assets/_SuperCommando_/Script/System/Services/Legacy`
- put UI view components in `Assets/_SuperCommando_/Script/GUI/...`

Do not add new cross-project static helpers for systems that already have service contracts.

## Remaining Legacy Areas

The project is not fully migrated yet. The largest remaining legacy areas are:

- `GlobalValue` still exists as a legacy compatibility bag, though its `PlayerPrefs` access is now funneled through `IKeyValueStore`
- the former `GameManager.Instance` / `MenuManager.Instance` / `CameraFollow.Instance` / `KeyUI.Instance` runtime access paths have been replaced in core gameplay with explicit services and legacy adapters
- `SoundManager` is now mostly isolated to `LegacyAudioService` and old commented template code, but the compatibility layer still exists
- `SoundManager` now primarily acts as the underlying concrete audio component instead of a cross-project static access point
- remaining `PlayerPrefs` hits are now concentrated in `UnityPrefsKeyValueStore` and a few explicit compatibility/bootstrap points rather than spread through gameplay code
- older enemy bases are partially migrated, but still need deeper extraction of combat/state responsibilities into reusable state-machine-driven components
- runtime scene state still centered around `GameManager`
- old ad classes still exist as implementation details behind adapters

## Recommended Next Steps

If the migration continues, the next bounded contexts should be:

1. `GameManager` runtime state extraction into a proper per-level session model instead of a legacy adapter over `GameManager`
2. AI/environment audio migration onto `IAudioService`
3. save/profile/progression persistence split away from `GlobalValue`
4. ad implementation cleanup so only `IAdsService` remains visible to game code
5. gradual removal of compatibility singletons once all callers are migrated

## Setup Notes

- VContainer is added through `Packages/manifest.json`
- after pulling/opening the project, let Unity reimport packages and scripts
- then check Unity Console for serialization or compile issues before continuing work

## Important Note About This Refactor

This migration was intentionally done in bounded slices. The goal was to raise architectural quality without destabilizing gameplay scenes and prefabs in one rewrite.
