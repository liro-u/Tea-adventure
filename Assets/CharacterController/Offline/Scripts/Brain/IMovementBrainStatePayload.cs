using UnityEngine;

public interface IMovementBrainStatePayload
{
    public bool ShouldWalk { get; set; }
    public bool ShouldSprint { get; set; }

    public float MovementSpeedModifier { get; set; }
    public float MovementDecelerationForce { get; set; }
    public int RemainingJump { get; set; }
    public Vector3 CurrentJumpForce { get; set; }

    public bool IsGrounded { get; set; }
    public bool IsMoving { get; set; }
    public bool IsStopping { get; set; }
    public bool IsLanding { get; set; }
}
