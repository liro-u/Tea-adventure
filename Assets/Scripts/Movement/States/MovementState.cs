using UnityEngine;
using UnityEngine.Windows;
using Unity.Netcode;

public enum MovementStateId
{
    Abstract,
    Idling,
    Walking,
    Running,
    Sprinting,
    LightStopping,
    MediumStopping,
    HardStopping,
    LightLanding,
    HardLanding,
    Rolling,
    Jumping,
    Falling,
}

public class MovementState : IState<MovementInputPayload, MovementStatePayload>
{
    public virtual MovementStateId StateId => MovementStateId.Abstract;

    protected MovementStateMachine stateMachine;

    protected readonly GroundedData groundedData;
    protected readonly AirborneData airborneData;

    public MovementState(MovementStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;

        groundedData = stateMachine.Data.GroundedData;
        airborneData = stateMachine.Data.AirborneData;
    }

    public virtual void Enter()
    {
        stateMachine.RawStatePayload.StateId = StateId;
    }

    public virtual void Exit()
    {
    }

    public MovementStatePayload Simulate() 
    {

        MovementStatePayload prevState = stateMachine.GetPrevNetworkState(stateMachine.currentInputPayload.Tick);

        Vector3 currentPosition = stateMachine.transform.position;
        stateMachine.characterController.enabled = false;
        stateMachine.transform.position = prevState.Position;
        stateMachine.characterController.enabled = true;

        SimulateTick();
        SimulatePhysicsTick();

        stateMachine.characterController.Move(stateMachine.RawStatePayload.Velocity);
        stateMachine.RawStatePayload.Position = stateMachine.transform.position;

        SimulateAfterMoveTick();

        stateMachine.characterController.enabled = false;
        stateMachine.transform.position = currentPosition;
        stateMachine.characterController.enabled = true;

        stateMachine.RawStatePayload.Tick = stateMachine.currentInputPayload.Tick;
        return stateMachine.RawStatePayload;
    }

    protected virtual void SimulateTick()
    {
        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero)
        {
            OnMoveCanceled();
        }

        if (stateMachine.currentInputPayload.IsJumping)
        {
            OnJumpStarted();
        }

        if (stateMachine.currentInputPayload.IsWalkToggle)
        {
            OnWalkToggleStarted();
        }
    }

    protected virtual void SimulatePhysicsTick()
    {
        Move();
    }

    protected virtual void SimulateAfterMoveTick()
    {
        if (stateMachine.characterController.isGrounded && !stateMachine.RawStatePayload.IsGrounded)
        {
            OnContactWithGround();
        }

        else if (!stateMachine.characterController.isGrounded && stateMachine.RawStatePayload.IsGrounded)
        {
            OnContactWithGroundExited();
        }
    }

    protected virtual void OnMoveCanceled()
    {
    }

    protected virtual void OnJumpStarted()
    {
        if (stateMachine.RawStatePayload.RemainingJump > 0)
        {
            stateMachine.ChangeState(stateMachine.JumpingState);
        }
    }
    protected virtual void OnWalkToggleStarted()
    {
        stateMachine.RawStatePayload.ShouldWalk = !stateMachine.RawStatePayload.ShouldWalk;
        Debug.LogError(stateMachine.RawStatePayload.ShouldWalk);
    }

    protected void Move()
    {
        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero || stateMachine.RawStatePayload.MovementSpeedModifier == 0f)
        {
            return;
        }

        // Horizontal movement
        Vector3 movementInputDirection = GetMovementInputDirection(stateMachine.currentInputPayload);

        stateMachine.RawStatePayload.TargetDirection = LocalToCameraDirection(movementInputDirection, stateMachine.currentInputPayload.CameraPivot);

        float movementSpeed = groundedData.BaseSpeed * stateMachine.RawStatePayload.MovementSpeedModifier * stateMachine.tickDelta;

        Vector3 currentPlayerHorizontalVelocity = GetHorizontalVelocity();

        AddForce(stateMachine.RawStatePayload.TargetDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange); 
    }

    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Acceleration)
    {
        switch (mode)
        {
            case ForceMode.Force:
            case ForceMode.Acceleration:
                // Continuous force per tick
                stateMachine.RawStatePayload.Velocity += force * stateMachine.tickDelta;
                break;
            case ForceMode.Impulse:
            case ForceMode.VelocityChange:
                // Instant velocity change
                stateMachine.RawStatePayload.Velocity += force;
                break;
        }
    }

    protected Vector3 GetHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = stateMachine.RawStatePayload.Velocity;

        playerHorizontalVelocity.y = 0f;

        return playerHorizontalVelocity;
    }

    protected Vector3 GetVerticalVelocity()
    {
        return new Vector3(0f, stateMachine.RawStatePayload.Velocity.y, 0f);
    }

    protected void ResetVerticalVelocity()
    {
        Vector3 playerHorizontalVelocity = GetHorizontalVelocity();

        stateMachine.RawStatePayload.Velocity = playerHorizontalVelocity;
    }

    protected void ResetVelocity()
    {
        stateMachine.RawStatePayload.Velocity = Vector3.zero;
    }

    protected Vector3 GetMovementInputDirection(MovementInputPayload input)
    {
        Vector3 movementInputDirection = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);
        movementInputDirection.Normalize();
        return movementInputDirection;
    }

    protected Vector3 LocalToCameraDirection(Vector3 movementInputDirection, Quaternion cameraPivot)
    {
        // Extract camera yaw
        Quaternion cameraYaw = Quaternion.Euler(0f, cameraPivot.eulerAngles.y, 0f);

        // Rotate movement by camera yaw
        Vector3 worldMovementDirection = cameraYaw * movementInputDirection;

        return worldMovementDirection;
    }

    protected bool IsMovingHorizontally(float minimumMagnitude = 0.1f)
    {
        Vector3 playerHorizontaVelocity = GetHorizontalVelocity();

        Vector2 playerHorizontalMovement = new Vector2(playerHorizontaVelocity.x, playerHorizontaVelocity.z);

        return playerHorizontalMovement.magnitude > minimumMagnitude;
    }

    protected bool IsMovingUp(float minimumVelocity = 0.1f)
    {
        return GetVerticalVelocity().y > minimumVelocity;
    }

    protected bool IsMovingDown(float minimumVelocity = 0.1f)
    {
        return GetVerticalVelocity().y < -minimumVelocity;
    }

    protected void DecelerateHorizontally()
    {
        Vector3 playerHorizontalVelocity = GetHorizontalVelocity();

        AddForce(-playerHorizontalVelocity * stateMachine.RawStatePayload.MovementDecelerationForce, ForceMode.Acceleration);
    }

    protected void DecelerateVertically()
    {
        Vector3 playerVerticalVelocity = GetVerticalVelocity();

        AddForce(-playerVerticalVelocity * stateMachine.RawStatePayload.MovementDecelerationForce, ForceMode.Acceleration);
    }

    public virtual void OnAnimationEnterEvent()
    {
    }

    public virtual void OnAnimationExitEvent()
    {
    }

    public virtual void OnAnimationTransitionEvent()
    {
    }

    protected virtual void OnContactWithGround()
    {
    }

    protected virtual void OnContactWithGroundExited()
    {
    }
}