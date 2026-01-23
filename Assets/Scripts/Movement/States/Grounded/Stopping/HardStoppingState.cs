using UnityEngine;

public class HardStoppingState : StoppingState
{
    public override MovementStateId StateId => MovementStateId.HardStopping;
    public HardStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawMovementStatePayload.MovementDecelerationForce = groundedData.StopData.HardDecelerationForce;

        stateMachine.RawMovementStatePayload.CurrentJumpForce = airborneData.JumpData.StrongForce;
    }

    public override void OnAnimationTransitionEvent()
    {
        base.OnAnimationTransitionEvent();

        stateMachine.ChangeState(stateMachine.IdlingState);
    }

    protected override void OnMove()
    {
    }
}
