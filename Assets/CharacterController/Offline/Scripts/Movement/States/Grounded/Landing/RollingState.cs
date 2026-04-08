using UnityEngine;
using UnityEngine.InputSystem;

public class RollingState : LandingState
{
    public RollingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = stateMachine.movementBrain.movementData.GroundedData.RollData.SpeedModifier;

        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = false;
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
            return;
        }
    }

    public override void OnAnimationTransitionEvent()
    {
        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MediumStoppingState);

            return;
        }

        OnMove();
    }

    protected override void OnJumpStarted()
    {
    }
}
