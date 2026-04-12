using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Full value-type snapshot of the player simulation at one tick.
///
/// Offline:  returned by CharacterBrainCore.SimulateTick but discarded.
/// Online:   buffered by ClientPrediction&lt;PlayerInputPayload, PlayerStateSnapshot&gt;
///           and sent as a ClientRpc parameter for server corrections.
///
/// Tick is managed externally by ClientPrediction and passed as a dedicated
/// RPC parameter — it does not need to be embedded in the snapshot.
/// </summary>
public struct PlayerStateSnapshot : INetworkSerializable
{
    // ── Motor ─────────────────────────────────────────────────────────────────
    public Vector3 Position;
    public Vector3 Velocity;

    // ── Movement state identity ───────────────────────────────────────────────
    public MovementStateId MovementStateId;

    // ── Movement payload ──────────────────────────────────────────────────────
    public bool    ShouldWalk;
    public bool    ShouldSprint;
    public float   MovementSpeedModifier;
    public float   MovementDecelerationForce;
    public int     RemainingJump;
    public Vector3 CurrentJumpForce;
    public bool    IsGrounded;
    public bool    IsMoving;
    public bool    IsStopping;
    public bool    IsLanding;
    public float   StateTimer;

    // ── INetworkSerializable ──────────────────────────────────────────────────

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Velocity);

        var stateId = (byte)MovementStateId;
        serializer.SerializeValue(ref stateId);
        if (serializer.IsReader) MovementStateId = (MovementStateId)stateId;

        serializer.SerializeValue(ref ShouldWalk);
        serializer.SerializeValue(ref ShouldSprint);
        serializer.SerializeValue(ref MovementSpeedModifier);
        serializer.SerializeValue(ref MovementDecelerationForce);
        serializer.SerializeValue(ref RemainingJump);
        serializer.SerializeValue(ref CurrentJumpForce);
        serializer.SerializeValue(ref IsGrounded);
        serializer.SerializeValue(ref IsMoving);
        serializer.SerializeValue(ref IsStopping);
        serializer.SerializeValue(ref IsLanding);
        serializer.SerializeValue(ref StateTimer);
    }
}
