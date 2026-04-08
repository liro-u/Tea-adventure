using UnityEngine;
public class AirborneState : MovementState
{
    public AirborneState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        ResetSprintState();
    }

    protected virtual void ResetSprintState()
    {
        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = false;
    }

    protected override void OnContactWithGround()
    {
        stateMachine.ChangeState(stateMachine.LightLandingState);
    }
}