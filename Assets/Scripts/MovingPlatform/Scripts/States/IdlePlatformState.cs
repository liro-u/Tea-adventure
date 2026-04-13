/// <summary>
/// Platform is stationary, waiting for the EventZone to be triggered.
/// Resets direction and target on entry so the platform always starts its
/// route from the beginning when re-triggered after a OneShot cycle.
/// </summary>
public class IdlePlatformState : MovingPlatformState
{
    public IdlePlatformState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.Brain.Motor.ResetVelocity();
        stateMachine.Brain.StatePayload.IsActivated         = false;
        stateMachine.Brain.StatePayload.WaypointDirection   = 1;
        stateMachine.Brain.StatePayload.TargetWaypointIndex = 0;
    }

    public override void Tick(float tickDelta)
    {
        if (!stateMachine.CurrentInput.IsTriggered) return;

        stateMachine.Brain.StatePayload.IsActivated = true;
        stateMachine.ChangeState(stateMachine.MovingState);
    }

    public override void Exit() { }
}
