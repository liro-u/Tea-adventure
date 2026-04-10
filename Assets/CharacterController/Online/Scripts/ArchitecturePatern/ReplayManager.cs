using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }

    [SerializeField] private float tickRate = 50f;
    public float TickDelta { get; private set; }
    public int CurrentTick { get; private set; }

    private readonly List<IReplayable> registered = new();
    private int pendingReplayFrom = int.MaxValue;
    private float tickTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        TickDelta = 1f / tickRate;
    }

    public void Register(IReplayable replayable) => registered.Add(replayable);
    public void Unregister(IReplayable replayable) => registered.Remove(replayable);

    public void MarkDirty(int fromTick)
    {
        if (fromTick < pendingReplayFrom)
            pendingReplayFrom = fromTick;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        while (tickTimer >= TickDelta)
        {
            tickTimer -= TickDelta;
            foreach (var r in registered)
                if (r.CanSimulate)
                    r.OnTick(CurrentTick);
            CurrentTick++;
        }
    }

    public void Rewind(int toTick)
    {
        foreach (var r in registered)
            r.RestoreState(toTick);
    }

    public void Replay(int fromTick, int toTick)
    {
        for (int tick = fromTick; tick < toTick; tick++)
            foreach (var r in registered)
                if (r.CanSimulate)
                    r.SimulateTick(tick);
    }

    private void LateUpdate()
    {
        if (pendingReplayFrom == int.MaxValue) return;

        int from = pendingReplayFrom;
        pendingReplayFrom = int.MaxValue;

        Replay(from, CurrentTick);
    }
}
