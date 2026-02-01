using UnityEngine;
using UnityEngine.InputSystem;

public class RollingState : LandingState
{
    public override MovementStateId StateId => MovementStateId.Rolling;

    public RollingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.MovementSpeedModifier = groundedData.RollData.SpeedModifier;

        base.Enter();

        stateMachine.RawStatePayload.ShouldSprint = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void SimulatePhysicsTick()
    {
        base.SimulatePhysicsTick();

        if (stateMachine.currentInputPayload.MoveInput != Vector2.zero)
        {
            return;
        }
    }

    public override void OnAnimationTransitionEvent()
    {
        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MediumStoppingState);

            return;
        }

        OnMove();
    }

    protected override void OnJumpStarted()
    {
    }
}
