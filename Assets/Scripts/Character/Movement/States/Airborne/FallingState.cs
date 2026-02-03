using System;
using UnityEngine;

public class FallingState : AirborneState
{
    private Vector3 positionOnEnter;

    public FallingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0;

        positionOnEnter = stateMachine.movementBrain.movementMotor.Position;


        stateMachine.movementBrain.movementMotor.ResetVerticalVelocity();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        LimitVerticalVelocity();
    }

    private void LimitVerticalVelocity()
    {
        Vector3 playerVerticalVelocity = stateMachine.movementBrain.movementMotor.GetVerticalVelocity();

        if (playerVerticalVelocity.y >= -stateMachine.movementBrain.movementData.AirborneData.FallData.FallSpeedLimit)
        {
            return;
        }

        Vector3 limitedVelocityForce = new Vector3(0f, -stateMachine.movementBrain.movementData.AirborneData.FallData.FallSpeedLimit - playerVerticalVelocity.y, 0f);

        stateMachine.movementBrain.movementMotor.AddForce(limitedVelocityForce, ForceMode.VelocityChange);
    }

    protected override void OnContactWithGround()
    {
        float fallDistance = positionOnEnter.y - stateMachine.movementBrain.movementMotor.Position.y;

        if (fallDistance < stateMachine.movementBrain.movementData.AirborneData.FallData.MinimumDistanceToBeConsideredHardFall)
        {
            stateMachine.ChangeState(stateMachine.LightLandingState);

            return;
        }

        if (stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk && !stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint || stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput == Vector2.zero)
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
