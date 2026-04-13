using UnityEngine;

/// <summary>
/// Full state snapshot of a moving platform at a given tick.
///
/// Offline: used by MovingPlatformBrainCore to implement ISimulatable (ApplyState / SimulateTick).
/// Online (future): promote to INetworkSerializable so ClientPrediction can buffer, compare,
/// and restore snapshots during reconciliation replay.
/// </summary>
public struct MovingPlatformStateSnapshot
{
    public Vector3               Position;
    public int                   TargetWaypointIndex;
    public int                   WaypointDirection;
    public float                 WaitTimer;
    public bool                  IsActivated;
    public MovingPlatformStateId StateId;
}
