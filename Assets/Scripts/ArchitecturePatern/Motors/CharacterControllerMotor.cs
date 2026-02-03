using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class CharacterControllerMotor : IMotor
{
    protected readonly CharacterController characterController;
    protected readonly float mass;
    protected readonly float gravity;
    protected readonly float groundFriction;
    protected readonly float airFriction;

    public CharacterControllerMotor(
        CharacterController characterController,
        float mass = 1f,
        float gravity = -9.81f,
        float groundFriction = 20f,
        float airFriction = 1f)
    {
        this.characterController = characterController;
        this.mass = mass;
        this.gravity = gravity;
        this.groundFriction = groundFriction;
        this.airFriction = airFriction;
    }

    public Vector3 Position
    {
        get => characterController.transform.position;
        set => SetPosition(value);
    }
    public Vector3 Velocity { get; protected set; }

    public Vector3 AccumulatedForce { get; protected set; }
    public Vector3 AccumulatedImpulse { get; protected set; }

    public bool IsGrounded => characterController.isGrounded;

    public float Height => characterController.height;
    public Vector3 Center => characterController.center;

    #region Public API
    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
    {
        switch (mode)
        {
            case ForceMode.Force:
                AccumulatedForce += force / mass;
                break;

            case ForceMode.Acceleration:
                AccumulatedForce += force;
                break;

            case ForceMode.Impulse:
                AccumulatedImpulse += force / mass;
                break;

            case ForceMode.VelocityChange:
                AccumulatedImpulse += force;
                break;
        }
    }

    public virtual void ApplyForce(float tickDelta, bool? isGrounded = null)
    {
        bool grounded = isGrounded ?? IsGrounded;

        // Add Gravity
        ApplyGravity(tickDelta, grounded);

        // Add Force and Impulse to Velocity
        Velocity += AccumulatedForce * tickDelta;
        Velocity += AccumulatedImpulse;

        // Add Friction
        ApplyFriction(tickDelta, grounded);

        // Move CharacterController
        characterController.Move(Velocity * tickDelta);

        // Sync with character controller velocity if collision has changed it
        if (Velocity != characterController.velocity)
        {
            Velocity = characterController.velocity;
        }

        // Clear forces
        AccumulatedForce = Vector3.zero;
        AccumulatedImpulse = Vector3.zero;
    }

    public Vector3 GetHorizontalVelocity()
    {
        return new Vector3(Velocity.x, 0, Velocity.z);
    }

    public Vector3 GetVerticalVelocity()
    {
        return new Vector3(0f, Velocity.y, 0f);
    }

    public void ResetVerticalVelocity()
    {
        Velocity = GetHorizontalVelocity();
    }

    public void ResetVelocity()
    {
        Velocity = Vector3.zero;
    }

    public bool IsMovingHorizontally(float minimumMagnitude = 0.1f)
    {
        Vector3 horizontaVelocity = GetHorizontalVelocity();

        Vector2 horizontalMovement = new Vector2(horizontaVelocity.x, horizontaVelocity.z);

        return horizontalMovement.magnitude > minimumMagnitude;
    }

    public bool IsMovingUp(float minimumVelocity = 0.1f)
    {
        return GetVerticalVelocity().y > minimumVelocity;
    }

    public bool IsMovingDown(float minimumVelocity = 0.1f)
    {
        return GetVerticalVelocity().y < -minimumVelocity;
    }

    public void DecelerateHorizontally(float decelerationForce)
    {
        AddForce(-GetHorizontalVelocity() * decelerationForce, ForceMode.Acceleration);
    }

    public void DecelerateVertically(float decelerationForce)
    {
        AddForce(-GetVerticalVelocity() * decelerationForce, ForceMode.Acceleration);
    }

    public void Debug()
    {
        Debuger.Instance.Add($"Position : {Position}");
        Debuger.Instance.Add($"Velocity : {Velocity}");
        Debuger.Instance.Add($"IsGrounded : {IsGrounded}");
    }
    #endregion

    protected void ApplyFriction(float tickDelta, bool isGrounded)
    {
        Vector3 horizontalVelocity = GetHorizontalVelocity();
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.001f)
            return;

        float friction = isGrounded ? groundFriction : airFriction;

        float drop = friction * tickDelta;
        float newSpeed = Mathf.Max(speed - drop, 0f);

        horizontalVelocity *= newSpeed / speed;

        Velocity = new Vector3(horizontalVelocity.x, Velocity.y, horizontalVelocity.z);

    }

    protected void ApplyGravity(float tickDelta, bool isGrounded)
    {
        if (isGrounded && Velocity.y < 0f)
            Velocity = new Vector3(Velocity.x, -2f, Velocity.z);
        else
            AddForce(new Vector3(0f, gravity, 0f), ForceMode.Force);

    }

    protected void SetPosition(Vector3 position)
    {
        characterController.enabled = false;
        characterController.transform.position = position;
        characterController.enabled = true;
    }
}
