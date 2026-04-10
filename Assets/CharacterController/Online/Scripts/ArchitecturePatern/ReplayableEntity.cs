public abstract class ReplayableEntity<TState> : IReplayable
    where TState : struct, ITickPayload
{
    public const int BUFFER_SIZE = 1024;
    protected float TickDelta => ReplayManager.Instance.TickDelta;

    protected TState[] stateBuffer = new TState[BUFFER_SIZE];

    public abstract bool CanSimulate { get; }

    public abstract void OnTick(int tick);

    public void RestoreState(int tick)
        => ApplyState(stateBuffer[tick % BUFFER_SIZE]);

    public abstract void SimulateTick(int tick);

    protected abstract void ApplyState(TState state);
}
