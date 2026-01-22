using UnityEngine;

public class WalkingState : MovingState
{
    public override MovementStateId StateId => MovementStateId.Walking;

    public WalkingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 1;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        StopWalking();
    }

    private void StopWalking()
    {
        if (stateMachine.currentInput.MoveInput == Vector2.zero)
        {
            return;
        }

        if (stateMachine.currentInput.IsRunning)
        {
            stateMachine.ChangeState(stateMachine.RunningState);

            return;
        }
    }

    protected override void OnMoveCanceled()
    {
        stateMachine.ChangeState(stateMachine.LightStoppingState);
    }
}