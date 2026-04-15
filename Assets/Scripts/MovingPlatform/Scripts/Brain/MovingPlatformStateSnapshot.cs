using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Full state snapshot of a moving platform at a given tick.
///
/// Offline: used by MovingPlatformBrainCore to implement ISimulatable (ApplyState / SimulateTick).
/// Online: buffered by NetworkMovingPlatformBrainCore and sent as a ClientRpc parameter
/// so all clients can reconcile against the server's authoritative state.
/// </summary>
public struct MovingPlatformStateSnapshot : INetworkSerializable
{
    public Vector3               Position;
    public float                 SplineT;
    public int                   TargetKnotIndex;
    public int                   WaypointDirection;
    public float                 WaitTimer;
    public bool                  IsActivated;
    public MovingPlatformStateId StateId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref SplineT);
        serializer.SerializeValue(ref TargetKnotIndex);
        serializer.SerializeValue(ref WaypointDirection);
        serializer.SerializeValue(ref WaitTimer);
        serializer.SerializeValue(ref IsActivated);
        var stateId = (byte)StateId;
        serializer.SerializeValue(ref stateId);
        if (serializer.IsReader) StateId = (MovingPlatformStateId)stateId;
    }
}
