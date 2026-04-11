using UnityEngine;

/// <summary>
/// A full, value-type snapshot of the player simulation at one tick.
/// Used as TState in ISimulatable&lt;PlayerInputPayload, PlayerStateSnapshot&gt;.
///
/// Offline:  returned by SimulateTick but otherwise discarded.
/// Online:   the NetworkCharacterBrain passes this to ClientPrediction as TState.
///           It must eventually implement INetworkSerializable — every field here
///           is a blittable value type so that conversion requires no structural change,
///           only adding the serialization methods.
///
/// When adding a new system (combat, vehicles, interaction), extend this struct
/// and add the corresponding RestoreXxx / SnapshotXxx helpers in CharacterBrain.
/// </summary>
public struct PlayerStateSnapshot
{
    // ── Motor ─────────────────────────────────────────────────────────────────
    public Vector3 Position;
    public Vector3 Velocity;

    // ── Movement state identity ───────────────────────────────────────────────
    public MovementStateId MovementStateId;

    // ── Movement payload ──────────────────────────────────────────────────────
    public bool   ShouldWalk;
    public bool   ShouldSprint;
    public float  MovementSpeedModifier;
    public float  MovementDecelerationForce;
    public int    RemainingJump;
    public Vector3 CurrentJumpForce;
    public bool   IsGrounded;
    public bool   IsMoving;
    public bool   IsStopping;
    public bool   IsLanding;

    // Replay-safe timer — replaces any Time.time usage inside states.
    // States accumulate this via tickDelta; snapshot/restore keeps it correct
    // across reconciliation replays.
    public float  StateTimer;
}
