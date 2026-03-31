using UnityEngine;
using VContainer;

public enum Attacks { None, Disappear, ThrowStone, SpeedAttack, SuperAttack, FallingObjAttack, FlyingAttack, FlyingThrow, FlyingSpreadBullet, TornadoAttack, Boomerang }

[System.Serializable]
public class AttackOrder
{
    public float delayMin = 1f;
    public float delayMax = 2f;
    public Attacks[] attackRandomList;
}

public class Boss1AttackOrder : MonoBehaviour
{
    private enum AttackLoopState
    {
        Idle,
        Delay,
        WaitForWindow,
        WaitForAttackCompletion
    }

    public BOSS_1 BossTarget;
    public AttackOrder[] attackOrders;

    private int current;
    private AttackLoopState state = AttackLoopState.Idle;
    private float delayTimer = -1f;
    private Attacks activeAttack = Attacks.None;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnDrawGizmos()
    {
        if (BossTarget == null && TryGetComponent(out BOSS_1 boss))
            BossTarget = boss;
    }

    private void OnEnable()
    {
        if (BossTarget == null && TryGetComponent(out BOSS_1 boss))
            BossTarget = boss;
    }

    private void Update()
    {
        if (BossTarget == null || attackOrders == null || attackOrders.Length == 0)
            return;

        if (state == AttackLoopState.Idle)
            return;

        if (BossTarget.isDead)
        {
            state = AttackLoopState.Idle;
            activeAttack = Attacks.None;
            return;
        }

        switch (state)
        {
            case AttackLoopState.Delay:
                TickDelay();
                break;
            case AttackLoopState.WaitForWindow:
                TickWaitForWindow();
                break;
            case AttackLoopState.WaitForAttackCompletion:
                TickWaitForAttackCompletion();
                break;
        }
    }

    public void Play()
    {
        if (attackOrders == null || attackOrders.Length == 0)
        {
            state = AttackLoopState.Idle;
            return;
        }

        current = Mathf.Clamp(current, 0, attackOrders.Length - 1);
        activeAttack = Attacks.None;
        ScheduleCurrentDelay();
    }

    private void TickDelay()
    {
        if (gameSession.State != GameManager.GameState.Playing)
            return;

        delayTimer -= Time.deltaTime;
        if (delayTimer <= 0f)
            state = AttackLoopState.WaitForWindow;
    }

    private void TickWaitForWindow()
    {
        if (gameSession.State != GameManager.GameState.Playing)
            return;

        if (!BossTarget.isPlayerInRange || BossTarget.isMeleeAttacking)
            return;

        ExecuteCurrentAttack();
    }

    private void TickWaitForAttackCompletion()
    {
        if (IsAttackRunning(activeAttack))
            return;

        AdvanceAttackOrder();
    }

    private void ExecuteCurrentAttack()
    {
        AttackOrder order = attackOrders[current];
        if (order.attackRandomList == null || order.attackRandomList.Length == 0)
        {
            AdvanceAttackOrder();
            return;
        }

        activeAttack = order.attackRandomList[Random.Range(0, order.attackRandomList.Length)];
        switch (activeAttack)
        {
            case Attacks.Disappear:
                BossTarget.DisappearShowAction();
                break;
            case Attacks.ThrowStone:
                BossTarget.ThrowStoneCoAction();
                break;
            case Attacks.SpeedAttack:
                BossTarget.SpeedAttackCoAction();
                break;
            case Attacks.SuperAttack:
                BossTarget.SuperAttackCoAction();
                break;
            case Attacks.FallingObjAttack:
                BossTarget.FallingObjectAttackCoAction();
                break;
            case Attacks.FlyingAttack:
                BossTarget.FlyingAttackCoAction();
                break;
            case Attacks.FlyingThrow:
                BossTarget.FlyingAttackCoAction(true);
                break;
            case Attacks.FlyingSpreadBullet:
                BossTarget.FlyingAttackCoAction(false, true);
                break;
            case Attacks.TornadoAttack:
                BossTarget.TORNADOAttackCoAction();
                break;
            case Attacks.Boomerang:
                BossTarget.BoomerangAttackCoAction();
                break;
            default:
                AdvanceAttackOrder();
                return;
        }

        state = AttackLoopState.WaitForAttackCompletion;
    }

    private bool IsAttackRunning(Attacks attack)
    {
        switch (attack)
        {
            case Attacks.Disappear:
                return BossTarget.disapearing;
            case Attacks.ThrowStone:
                return BossTarget.IsIdleDelayActive;
            case Attacks.SpeedAttack:
                return BossTarget.isAttackSpeed;
            case Attacks.SuperAttack:
                return BossTarget.isSuperAttacking;
            case Attacks.FallingObjAttack:
                return BossTarget.isFallingObjectAttack;
            case Attacks.FlyingAttack:
            case Attacks.FlyingThrow:
            case Attacks.FlyingSpreadBullet:
                return BossTarget.isFlyingAttack;
            case Attacks.TornadoAttack:
                return BossTarget.isTornadoAttacking;
            case Attacks.Boomerang:
                return BossTarget.isBoomerangeAttacking;
            default:
                return false;
        }
    }

    private void AdvanceAttackOrder()
    {
        activeAttack = Attacks.None;
        current++;
        if (current >= attackOrders.Length)
            current = 0;

        ScheduleCurrentDelay();
    }

    private void ScheduleCurrentDelay()
    {
        AttackOrder order = attackOrders[current];
        delayTimer = Random.Range(order.delayMin, order.delayMax);
        state = AttackLoopState.Delay;
    }
}
