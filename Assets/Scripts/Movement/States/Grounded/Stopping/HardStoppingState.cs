using UnityEngine;

public class HardStoppingState : StoppingState
{
    public override MovementStateId StateId => MovementStateId.HardStopping;
    public HardStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
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
