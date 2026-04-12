using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Concrete input struct for the player character.
/// Implements IMovementInputPayload for the state machine and
/// INetworkSerializable so ClientPrediction can use it as TInput in RPCs.
///
/// Tick is managed externally by ClientPrediction and passed as a dedicated
/// RPC parameter — it does not need to be part of the simulation payload.
/// </summary>
public struct PlayerInputPayload : IMovementInputPayload, INetworkSerializable
{
    // Backing fields for ref access in NetworkSerialize.
    private Vector2    moveInput;
    private Vector2    lookInput;
    private Quaternion cameraPivot;
    private bool       isSprinting;
    private bool       isWalkToggle;
    private bool       isJumping;

    // ── IMovementInputPayload ─────────────────────────────────────────────────

    public Vector2    MoveInput    { get => moveInput;    set => moveInput    = value; }
    public Vector2    LookInput    { get => lookInput;    set => lookInput    = value; }
    public Quaternion CameraPivot  { get => cameraPivot;  set => cameraPivot  = value; }
    public bool       IsSprinting  { get => isSprinting;  set => isSprinting  = value; }
    public bool       IsWalkToggle { get => isWalkToggle; set => isWalkToggle = value; }
    public bool       IsJumping    { get => isJumping;    set => isJumping    = value; }

    // ── INetworkSerializable ──────────────────────────────────────────────────

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref moveInput);
        serializer.SerializeValue(ref lookInput);
        serializer.SerializeValue(ref cameraPivot);
        serializer.SerializeValue(ref isSprinting);
        serializer.SerializeValue(ref isWalkToggle);
        serializer.SerializeValue(ref isJumping);
    }
}
