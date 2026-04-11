using UnityEngine;

public class LightLandingState : LandingState
{
    public LightLandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0;

        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce =
            stateMachine.movementBrain.movementData.AirborneData.JumpData.StationaryForce;

        stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.CurrentInput.MoveInput != Vector2.zero)
            OnMove();

        if (stateMachine.movementBrain.movementMotor.IsMovingHorizontally())
            stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void OnAnimationTransitionEvent()
    {
        stateMachine.ChangeState(stateMachine.IdlingState);
    }
}
