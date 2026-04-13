using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Concrete input struct for the player character.
/// INetworkSerializable so ClientPrediction can use it as TInput in RPCs.
///
/// Tick is managed externally by ClientPrediction and passed as a dedicated
/// RPC parameter — it does not need to be part of the simulation payload.
/// </summary>
public struct PlayerInputPayload : INetworkSerializable
{
    public Vector2    MoveInput;
    public Vector2    LookInput;
    public Quaternion CameraPivot;
    public bool       IsSprinting;
    public bool       IsWalkToggle;
    public bool       IsJumping;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MoveInput);
        serializer.SerializeValue(ref LookInput);
        serializer.SerializeValue(ref CameraPivot);
        serializer.SerializeValue(ref IsSprinting);
        serializer.SerializeValue(ref IsWalkToggle);
        serializer.SerializeValue(ref IsJumping);
    }
}
