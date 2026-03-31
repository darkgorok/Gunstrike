public sealed class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }

    public void ChangeState(IEnemyState nextState)
    {
        if (ReferenceEquals(CurrentState, nextState))
            return;

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState?.Enter();
    }

    public void Tick(float deltaTime)
    {
        CurrentState?.Tick(deltaTime);
    }
}
