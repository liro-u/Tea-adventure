public interface IReplayable
{
    bool CanSimulate { get; }

    void OnTick(int tick);
    void RestoreState(int tick);
    void SimulateTick(int tick);
}
