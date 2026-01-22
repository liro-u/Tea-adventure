using UnityEngine;
public class IdlingState : GroundedState
{
    public override MovementStateId StateId  => MovementStateId.Idling;

    public IdlingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (stateMachine.currentInput.MoveInput == Vector2.zero)
        {
            return;
        }

        OnMove();
    }
}