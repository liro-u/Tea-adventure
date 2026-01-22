using UnityEngine;
public class RunningState : MovingState
{
    public override MovementStateId StateId => MovementStateId.Running;

    public RunningState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 5;

        base.Enter();
    }

    public override void Exit()
    {
        stateMachine.RawMovementInputPayload.IsRunning = false;

        base.Exit();
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();
    }

    protected override void OnMoveCanceled()
    {
        stateMachine.ChangeState(stateMachine.HardStoppingState);
    }
}