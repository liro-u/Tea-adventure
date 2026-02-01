using UnityEngine;

public class MediumStoppingState : StoppingState
{
    public override MovementStateId StateId => MovementStateId.MediumStopping;
    public MediumStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawStatePayload.MovementDecelerationForce = groundedData.StopData.MediumDecelerationForce;

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.MediumForce;
    }
}
