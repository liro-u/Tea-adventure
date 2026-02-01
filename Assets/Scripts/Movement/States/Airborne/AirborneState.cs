using UnityEngine;
public class AirborneState : MovementState
{
    public AirborneState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        ResetSprintState();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected virtual void ResetSprintState()
    {
        stateMachine.RawStatePayload.ShouldSprint = false;
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();
    }

    protected override void SimulatePhysicsTick()
    {
        base.SimulatePhysicsTick();

        ApplyGravity();
    }

    protected void ApplyGravity()
    {
        AddForce(stateMachine.Data.AirborneData.Gravity, ForceMode.Acceleration);
    }

    protected override void OnContactWithGround()
    {
        stateMachine.ChangeState(stateMachine.LightLandingState);
    }
}