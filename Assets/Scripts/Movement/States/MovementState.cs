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
    Jumping
}

public class MovementState : IState<MovementInputPayload, MovementStatePayload>
{
    public virtual MovementStateId StateId => MovementStateId.Abstract;

    protected MovementStateMachine stateMachine;

    public MovementState(MovementStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
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
        Move();

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
        // Horizontal movement
        Vector3 movementInputDirection = GetMovementInputDirection(stateMachine.currentInput);

        Vector3 worldMovementDirection = LocalToCameraDirection(movementInputDirection, stateMachine.currentInput.CameraPivot);

        Vector3 horizontalVelocity = worldMovementDirection * stateMachine.RawMovementStatePayload.MovementSpeedModifier;

        Vector3 velocity = new Vector3(
            horizontalVelocity.x,
            -9.8f,
            horizontalVelocity.z
        );

        Vector3 displacement = velocity * stateMachine.tickDelta;

        stateMachine.characterController.Move(displacement);
        stateMachine.RawMovementStatePayload.Displacement = displacement; 
        stateMachine.RawMovementStatePayload.Position = stateMachine.transform.position; 
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

    public virtual void OnAnimationEnterEvent()
    {
    }

    public virtual void OnAnimationExitEvent()
    {
    }

    public virtual void OnAnimationTransitionEvent()
    {
    }
}