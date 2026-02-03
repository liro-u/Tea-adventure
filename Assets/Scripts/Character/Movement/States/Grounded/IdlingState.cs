using UnityEngine;
public class IdlingState : GroundedState
{
    public IdlingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.StationaryForce;

        base.Enter();

        stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput != Vector2.zero)
        {
            OnMove();
        }

        if (stateMachine.movementBrain.movementMotor.IsMovingHorizontally())
        {
            stateMachine.movementBrain.movementMotor.ResetVelocity();
        }

    }
}