public sealed class LegacyPlayerProfileService : IPlayerProfileService
{
    private const string SelectedCharacterInstanceIdKey = "ChoosenCharacterInstanceID";
    private const string SelectedCharacterIdKey = "choosenCharacterID";
    private const string CharacterKeyPrefix = "Character";

    private readonly IKeyValueStore keyValueStore;

    public LegacyPlayerProfileService(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    public int SelectedCharacterInstanceId
    {
        get => keyValueStore.GetInt(SelectedCharacterInstanceIdKey, 0);
        set => keyValueStore.SetInt(SelectedCharacterInstanceIdKey, value);
    }

    public int SelectedCharacterId
    {
        get => keyValueStore.GetInt(SelectedCharacterIdKey, 1);
        set => keyValueStore.SetInt(SelectedCharacterIdKey, value);
    }

    public bool IsCharacterUnlocked(int characterId)
    {
        return keyValueStore.GetInt(CharacterKeyPrefix + characterId, 0) == 1;
    }

    public void SetCharacterUnlocked(int characterId, bool unlocked)
    {
        keyValueStore.SetInt(CharacterKeyPrefix + characterId, unlocked ? 1 : 0);
    }
}
