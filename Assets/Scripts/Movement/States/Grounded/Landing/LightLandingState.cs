using UnityEngine;

public class LightLandingState : LandingState
{
    public override MovementStateId StateId => MovementStateId.LightLanding;

    public LightLandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.MovementSpeedModifier = 0;

        base.Enter();

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.StationaryForce;

        ResetVelocity();
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero)
        {
            return;
        }

        OnMove();
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

    public override void OnAnimationTransitionEvent()
    {
        stateMachine.ChangeState(stateMachine.IdlingState);
    }
}
