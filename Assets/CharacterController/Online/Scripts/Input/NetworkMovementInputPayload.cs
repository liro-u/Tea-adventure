using Unity.Netcode;
using UnityEngine;

public struct NetworkMovementInputPayload : IMovementInputPayload, ITickPayload, INetworkSerializeByMemcpy
{
    public Vector2 MoveInput { get; set; }
    public Vector2 LookInput { get; set; }
    public Quaternion CameraPivot { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsWalkToggle { get; set; }
    public bool IsJumping { get; set; }
    public int Tick { get; set; }
}
