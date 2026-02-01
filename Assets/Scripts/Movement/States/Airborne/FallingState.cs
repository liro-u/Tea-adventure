using System;
using UnityEngine;

public class FallingState : AirborneState
{
    public override MovementStateId StateId => MovementStateId.Falling;

    private Vector3 positionOnEnter;

    public FallingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawStatePayload.MovementSpeedModifier = 0;

        positionOnEnter = stateMachine.transform.position;


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
        Vector3 playerVerticalVelocity = GetVerticalVelocity();

        if (playerVerticalVelocity.y >= -airborneData.FallData.FallSpeedLimit)
        {
            return;
        }

        Vector3 limitedVelocityForce = new Vector3(0f, -airborneData.FallData.FallSpeedLimit - playerVerticalVelocity.y, 0f);

        AddForce(limitedVelocityForce, ForceMode.VelocityChange);
    }

    protected override void OnContactWithGround()
    {
        float fallDistance = positionOnEnter.y - stateMachine.transform.position.y;

        if (fallDistance < airborneData.FallData.MinimumDistanceToBeConsideredHardFall)
        {
            stateMachine.ChangeState(stateMachine.LightLandingState);

            return;
        }

        if (stateMachine.RawStatePayload.ShouldWalk && !stateMachine.RawStatePayload.ShouldSprint || stateMachine.currentInputPayload.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.HardLandingState);

            return;
        }

        stateMachine.ChangeState(stateMachine.RollingState);

    }

    protected override void ResetSprintState()
    {
    }
}
