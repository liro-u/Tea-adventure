using UnityEngine;
using UnityEngine.InputSystem;

public class HardLandingState : LandingState
{
    public HardLandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    bool canMove;
    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0f;

        canMove = false;

        base.Enter();

        stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput != Vector2.zero)
        {
            OnMovementStarted();
        }

        if (stateMachine.movementBrain.movementMotor.IsMovingHorizontally())
        {
            stateMachine.movementBrain.movementMotor.ResetVelocity();
        }
    }

    public override void OnAnimationExitEvent()
    {
        canMove = true;
    }

    public override void OnAnimationTransitionEvent()
    {
        stateMachine.ChangeState(stateMachine.IdlingState);
    }

    protected void OnMovementStarted()
    {
        OnMove();
    }

    protected override void OnMove()
    {
        if (!canMove)
        {
            return;
        }

        if (stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk)
        {
            return;
        }

        stateMachine.ChangeState(stateMachine.RunningState);
    }

    protected override void OnJumpStarted()
    {
    }
}
