using UnityEngine;
using UnityEngine.InputSystem;

public class HardLandingState : LandingState
{
    public override MovementStateId StateId => MovementStateId.HardLanding;
    public HardLandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    bool canMove;
    public override void Enter()
    {
        stateMachine.RawStatePayload.MovementSpeedModifier = 0f;

        canMove = false;

        base.Enter();

        ResetVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (stateMachine.currentInputPayload.MoveInput != Vector2.zero)
        {
            OnMovementStarted();
        }
    }

    protected override void SimulatePhysicsTick()
    {
        base.SimulatePhysicsTick();

        if (!IsMovingHorizontally())
        {
            return;
        }

        ResetVelocity();
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

        if (stateMachine.RawStatePayload.ShouldWalk)
        {
            return;
        }

        stateMachine.ChangeState(stateMachine.RunningState);
    }

    protected override void OnJumpStarted()
    {
    }
}
