using UnityEngine;

/// <summary>
/// The single concrete input struct for the player character.
/// Implements IMovementInputPayload now; add more input interfaces here
/// when new systems (combat, interaction, etc.) are introduced.
///
/// Kept as a struct so the online layer can eventually make it
/// INetworkSerializable without any structural change.
/// </summary>
public struct PlayerInputPayload : IMovementInputPayload
{
    public Vector2 MoveInput { get; set; }
    public Vector2 LookInput { get; set; }
    public Quaternion CameraPivot { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsWalkToggle { get; set; }
    public bool IsJumping { get; set; }
}
