using Unity.Netcode;
using UnityEngine;

public struct MovementInputPayload
{
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public Quaternion CameraPivot;
    public bool IsSprinting;
    public bool IsWalkToggle;
    public bool IsJumping;
}

