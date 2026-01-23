using System;
using UnityEngine;

public class FallingState : AirborneState
{
    public override MovementStateId StateId => MovementStateId.Falling;

    public FallingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        ResetVerticalVelocity();
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

        LimitVerticalVelocity();
    }

    private void LimitVerticalVelocity()
    {
        Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

        if (playerVerticalVelocity.y >= -airborneData.FallData.FallSpeedLimit)
        {
            return;
        }

        Vector3 limitedVelocityForce = new Vector3(0f, -airborneData.FallData.FallSpeedLimit - playerVerticalVelocity.y, 0f);

        AddForce(limitedVelocityForce, ForceMode.VelocityChange);
    }

    protected override void OnContactWithGround()
    {
        stateMachine.ChangeState(stateMachine.IdlingState);
    }
}
