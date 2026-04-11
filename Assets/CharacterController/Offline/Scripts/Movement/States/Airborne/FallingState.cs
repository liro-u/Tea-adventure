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

    protected override void OnContactWithGround()
    {
        float fallDistance = positionOnEnter.y - stateMachine.movementBrain.movementMotor.Position.y;

        if (fallDistance < stateMachine.movementBrain.movementData.AirborneData.FallData.MinimumDistanceToBeConsideredHardFall)
        {
            stateMachine.ChangeState(stateMachine.LightLandingState);
            return;
        }

        bool isMoving = stateMachine.CurrentInput.MoveInput != Vector2.zero;
        bool isSprinting = stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint;
        bool shouldWalk = stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk;

        if (!isMoving || (shouldWalk && !isSprinting))
        {
            stateMachine.ChangeState(stateMachine.HardLandingState);
            return;
        }

        stateMachine.ChangeState(stateMachine.RollingState);
    }

    protected override void ResetSprintState() { }

    private void LimitVerticalVelocity()
    {
        Vector3 verticalVelocity = stateMachine.movementBrain.movementMotor.GetVerticalVelocity();
        float limit = stateMachine.movementBrain.movementData.AirborneData.FallData.FallSpeedLimit;

        if (verticalVelocity.y >= -limit) return;

        stateMachine.movementBrain.movementMotor.AddForce(
            new Vector3(0f, -limit - verticalVelocity.y, 0f),
            UnityEngine.ForceMode.VelocityChange);
    }
}
