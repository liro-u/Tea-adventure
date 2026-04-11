using UnityEngine;

public class HardLandingState : LandingState
{
    // Local bool intentionally NOT in the payload: it is reset to false on every Enter(),
    // and only set to true by an animation event fired from Update — not from the
    // simulation tick. This makes it safe for replay (Enter resets it, animation catches
    // up on the first Update after reconciliation ends). If this ever causes visible
    // desync it can be promoted to IMovementBrainStatePayload.
    private bool canMove;

    public HardLandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0f;
        canMove = false;

        base.Enter();

        stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.CurrentInput.MoveInput != Vector2.zero)
            OnMovementStarted();

        if (stateMachine.movementBrain.movementMotor.IsMovingHorizontally())
            stateMachine.movementBrain.movementMotor.ResetVelocity();
    }

    public override void OnAnimationExitEvent()
    {
        canMove = true;
    }

    public override void OnAnimationTransitionEvent()
    {
        stateMachine.ChangeState(stateMachine.IdlingState);
    }

    protected void OnMovementStarted()
    {
        OnMove();
    }

    protected override void OnMove()
    {
        if (!canMove) return;
        if (stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk) return;

        stateMachine.ChangeState(stateMachine.RunningState);
    }

    protected override void OnJumpStarted() { }
}
