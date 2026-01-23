using UnityEngine;
public class AirborneState : MovementState
{
    public AirborneState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
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
}