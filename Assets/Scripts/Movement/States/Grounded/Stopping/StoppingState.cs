using UnityEngine;
using UnityEngine.InputSystem;

public class StoppingState : GroundedState
{
    public StoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.IsStopping = true;

        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawMovementStatePayload.IsStopping = false;
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (stateMachine.RawMovementInputPayload.MoveInput != Vector2.zero)
        {
            OnMovementStarted();
        }

        if (!IsMovingHorizontally(0f))
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
