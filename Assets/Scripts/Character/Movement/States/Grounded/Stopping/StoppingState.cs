using UnityEngine;
using UnityEngine.InputSystem;

public class StoppingState : GroundedState
{
    public StoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.IsStopping = true;

        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.movementBrain.movementBrainStatePayload.IsStopping = false;
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput != Vector2.zero)
        {
            OnMovementStarted();
        }

        if (!stateMachine.movementBrain.movementMotor.IsMovingHorizontally(0f))
        {
            return;
        }

        DecelerateHorizontally();

    }

    public override void OnAnimationTransitionEvent()
    {
        base.OnAnimationTransitionEvent();

        stateMachine.ChangeState(stateMachine.IdlingState);
    }

    private void OnMovementStarted()
    {
        OnMove();
    }
}
