
using UnityEngine;

public class GroundedState : MovementState
{
    public GroundedState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.IsGrounded = true;

        stateMachine.movementBrain.movementBrainStatePayload.RemainingJump = stateMachine.movementBrain.movementData.AirborneData.JumpData.MaxConsecutiveJump;

        base.Enter();

        stateMachine.movementBrain.movementMotor.OnFall += OnFall;

        UpdateShouldSprintState();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.movementBrain.movementMotor.OnFall -= OnFall;

        stateMachine.movementBrain.movementBrainStatePayload.IsGrounded = false;
    }

    private void UpdateShouldSprintState()
    {
        if (!stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint)
        {
            return;
        }

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput != Vector2.zero)
        {
            return;
        }

        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = false;
    }

    protected virtual void OnMove()
    {
        if (stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk)
        {
            stateMachine.ChangeState(stateMachine.WalkingState);

            return;
        }
        if (stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);

            return;
        }
        
        stateMachine.ChangeState(stateMachine.RunningState);
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        stateMachine.movementBrain.movementMotor.StickToGround();
    }

    protected virtual void OnFall()
    {
        stateMachine.ChangeState(stateMachine.FallingState);
    }

}