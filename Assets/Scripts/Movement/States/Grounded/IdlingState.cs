using UnityEngine;
public class IdlingState : GroundedState
{
    public override MovementStateId StateId  => MovementStateId.Idling;

    public IdlingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        stateMachine.RawMovementStatePayload.CurrentJumpForce = airborneData.JumpData.StationaryForce;

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

        if (stateMachine.currentInput.MoveInput == Vector2.zero)
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
}