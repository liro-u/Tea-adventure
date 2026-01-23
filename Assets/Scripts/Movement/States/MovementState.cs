using UnityEngine;
using UnityEngine.Windows;
using Unity.Netcode;

public enum MovementStateId
{
    Abstract,
    Idling,
    Walking,
    Running,
    LightStopping,
    HardStopping,
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
        stateMachine.RawMovementStatePayload.StateId = StateId;
    }

    public virtual void Exit()
    {
    }

    public MovementStatePayload Simulate() 
    {
        MovementStatePayload prevState = stateMachine.GetPrevNetworkState(stateMachine.currentInput.Tick);

        Vector3 currentPosition = stateMachine.transform.position;
        stateMachine.characterController.enabled = false;
        stateMachine.transform.position = prevState.Position;
        stateMachine.characterController.enabled = true;

        SimulateTick();
        SimulatePhysicsTick();

        stateMachine.characterController.Move(stateMachine.RawMovementStatePayload.Velocity);
        stateMachine.RawMovementStatePayload.Position = stateMachine.transform.position;

        SimulateAfterMoveTick();

        stateMachine.characterController.enabled = false;
        stateMachine.transform.position = currentPosition;
        stateMachine.characterController.enabled = true;

        stateMachine.RawMovementStatePayload.Tick = stateMachine.currentInput.Tick;
        return stateMachine.RawMovementStatePayload;
    }

    protected virtual void SimulateTick()
    {
        if (stateMachine.currentInput.MoveInput == Vector2.zero)
        {
            OnMoveCanceled();
        }

        if (stateMachine.currentInput.IsJumping)
        {
            OnJumpStarted();
        }
    }

    protected virtual void SimulatePhysicsTick()
    {
        Move();
    }

    protected virtual void SimulateAfterMoveTick()
    {
        if (stateMachine.characterController.isGrounded && !stateMachine.RawMovementStatePayload.IsGrounded)
        {
            OnContactWithGround();
        }

        else if (!stateMachine.characterController.isGrounded && stateMachine.RawMovementStatePayload.IsGrounded)
        {
            OnContactWithGroundExited();
        }
    }

    protected virtual void OnMoveCanceled()
    {
    }

    protected virtual void OnJumpStarted()
    {
        if (stateMachine.RawMovementStatePayload.RemainingJump > 0)
        {
            stateMachine.ChangeState(stateMachine.JumpingState);
        }
    }

    protected void Move()
    {
        if (stateMachine.RawMovementInputPayload.MoveInput == Vector2.zero || stateMachine.RawMovementStatePayload.MovementSpeedModifier == 0f)
        {
            return;
        }

        // Horizontal movement
        Vector3 movementInputDirection = GetMovementInputDirection(stateMachine.currentInput);

        stateMachine.RawMovementStatePayload.TargetDirection = LocalToCameraDirection(movementInputDirection, stateMachine.currentInput.CameraPivot);

        float movementSpeed = groundedData.BaseSpeed * stateMachine.RawMovementStatePayload.MovementSpeedModifier * stateMachine.tickDelta;

        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();

        AddForce(stateMachine.RawMovementStatePayload.TargetDirection * movementSpeed - currentPlayerHorizontalVelocity, ForceMode.VelocityChange); 
    }

    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Acceleration)
    {
        switch (mode)
        {
            case ForceMode.Force:
            case ForceMode.Acceleration:
                // Continuous force per tick
                stateMachine.RawMovementStatePayload.Velocity += force * stateMachine.tickDelta;
                break;
            case ForceMode.Impulse:
            case ForceMode.VelocityChange:
                // Instant velocity change
                stateMachine.RawMovementStatePayload.Velocity += force;
                break;
        }
    }

    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = stateMachine.RawMovementStatePayload.Velocity;

        playerHorizontalVelocity.y = 0f;

        return playerHorizontalVelocity;
    }

    protected Vector3 GetPlayerVerticalVelocity()
    {
        return new Vector3(0f, stateMachine.RawMovementStatePayload.Velocity.y, 0f);
    }

    protected void ResetVerticalVelocity()
    {
        Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

        stateMachine.RawMovementStatePayload.Velocity = playerHorizontalVelocity;
    }

    protected void ResetVelocity()
    {
        stateMachine.RawMovementStatePayload.Velocity = Vector3.zero;
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
        Vector3 playerHorizontaVelocity = GetPlayerHorizontalVelocity();

        Vector2 playerHorizontalMovement = new Vector2(playerHorizontaVelocity.x, playerHorizontaVelocity.z);

        return playerHorizontalMovement.magnitude > minimumMagnitude;
    }

    protected bool IsMovingUp(float minimumVelocity = 0.1f)
    {
        return GetPlayerVerticalVelocity().y > minimumVelocity;
    }

    protected bool IsMovingDown(float minimumVelocity = 0.1f)
    {
        return GetPlayerVerticalVelocity().y < -minimumVelocity;
    }

    protected void DecelerateHorizontally()
    {
        Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

        AddForce(-playerHorizontalVelocity * stateMachine.RawMovementStatePayload.MovementDecelerationForce, ForceMode.Acceleration);
    }

    protected void DecelerateVertically()
    {
        Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

        AddForce(-playerVerticalVelocity * stateMachine.RawMovementStatePayload.MovementDecelerationForce, ForceMode.Acceleration);
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