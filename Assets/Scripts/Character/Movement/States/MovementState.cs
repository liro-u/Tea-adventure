using UnityEngine;

public class MovementState : State<MovementStateMachine>
{
    public MovementState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    #region Public API
    public override void Enter()
    {
        stateMachine.movementBrain.movementMotor.OnContactWithGround += OnContactWithGround;
        stateMachine.movementBrain.movementMotor.OnContactWithGroundExited += OnContactWithGroundExited;

        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationEnterEvent += OnAnimationEnterEvent;
        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationTransitionEvent += OnAnimationTransitionEvent;
        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationExitEvent += OnAnimationExitEvent;
    }

    public override void Exit()
    {
        stateMachine.movementBrain.movementMotor.OnContactWithGround -= OnContactWithGround;
        stateMachine.movementBrain.movementMotor.OnContactWithGroundExited -= OnContactWithGroundExited;

        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationEnterEvent -= OnAnimationEnterEvent;
        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationTransitionEvent -= OnAnimationTransitionEvent;
        stateMachine.movementBrain.MovementAnimationEventTrigger.OnAnimationExitEvent -= OnAnimationExitEvent;
    }

    public override void Tick(float tickDelta) 
    {
        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput == Vector2.zero)
        {
            OnMoveCanceled();
        }

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.IsJumping)
        {
            OnJumpStarted();
        }

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.IsWalkToggle)
        {
            OnWalkToggleStarted();
        }

        stateMachine.movementBrain.movementMotor.Move(
            stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput,
            stateMachine.movementBrain.movementData.GroundedData.BaseSpeed,
            stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier,
            stateMachine.movementBrain.movementInputProvider.InputPayload.CameraPivot
        );
    }
    #endregion

    protected void DecelerateHorizontally()
    {
        stateMachine.movementBrain.movementMotor.DecelerateHorizontally(stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce);
    }

    protected void DecelerateVertically()
    {
        stateMachine.movementBrain.movementMotor.DecelerateVertically(stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce);
    }

    protected virtual void OnContactWithGround()
    {
    }

    protected virtual void OnContactWithGroundExited()
    {
    }

    protected virtual void OnMoveCanceled()
    {
    }

    protected virtual void OnJumpStarted()
    {
        if (stateMachine.movementBrain.movementBrainStatePayload.RemainingJump > 0)
        {
            stateMachine.ChangeState(stateMachine.JumpingState);
        }
    }
    protected virtual void OnWalkToggleStarted()
    {
        stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk = !stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk;
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