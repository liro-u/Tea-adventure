using UnityEngine;

public interface IMotor
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; }

    public Vector3 AccumulatedForce { get; }
    public Vector3 AccumulatedImpulse { get; }

    public bool IsGrounded { get; }

    public float Height { get; }
    public Vector3 Center { get; }

    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force);
    public void ApplyForce(float tickDelta, bool? isGrounded);

    public Vector3 GetHorizontalVelocity();
    public Vector3 GetVerticalVelocity();

    public void ResetVerticalVelocity();
    public void ResetVelocity();

    public bool IsMovingHorizontally(float minimumMagnitude = 0.1f);
    public bool IsMovingUp(float minimumVelocity = 0.1f);
    public bool IsMovingDown(float minimumVelocity = 0.1f);

    public void DecelerateHorizontally(float decelerationForce);
    public void DecelerateVertically(float decelerationForce);
}