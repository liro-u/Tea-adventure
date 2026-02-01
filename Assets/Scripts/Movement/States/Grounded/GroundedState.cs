
using UnityEngine;

public class GroundedState : MovementState
{
    public GroundedState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.IsGrounded = true;

        stateMachine.RawStatePayload.RemainingJump = airborneData.JumpData.MaxConsecutiveJump;

        base.Enter();

        UpdateShouldSprintState();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawStatePayload.IsGrounded = false;
    }

    private void UpdateShouldSprintState()
    {
        if (!stateMachine.RawStatePayload.ShouldSprint)
        {
            return;
        }

        if (stateMachine.currentInputPayload.MoveInput != Vector2.zero)
        {
            return;
        }

        stateMachine.RawStatePayload.ShouldSprint = false;
    }

    protected virtual void OnMove()
    {
        if (stateMachine.RawStatePayload.ShouldWalk)
        {
            stateMachine.ChangeState(stateMachine.WalkingState);

            return;
        }
        if (stateMachine.RawStatePayload.ShouldSprint)
        {
            stateMachine.ChangeState(stateMachine.SprintingState);

            return;
        }
        
        stateMachine.ChangeState(stateMachine.RunningState);
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();
    }

    protected override void SimulatePhysicsTick()
    {
        base.SimulatePhysicsTick();

        StickToGround();
    }


    protected void StickToGround()
    {
        Vector3 capsuleColliderCenterInWorldSpace = stateMachine.transform.TransformPoint(stateMachine.characterController.center);

        Ray downwardsRayFromCapsuleCenter = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);

        if (Physics.Raycast(downwardsRayFromCapsuleCenter, out RaycastHit hit, groundedData.StickToGroundRayDistance, groundedData.GroundLayer, QueryTriggerInteraction.Ignore))
        {
            float centerToBottom = stateMachine.characterController.height * 0.5f;
            float distanceToGround = hit.distance - centerToBottom;

            if (Mathf.Abs(distanceToGround) < 0.02f)
            {
                return;
            }

            float amountToPull = distanceToGround;

            stateMachine.RawStatePayload.Velocity.y = -amountToPull;
        }
    }

    protected override void OnContactWithGroundExited()
    {


        Vector3 capsuleColliderCenterInWorldSpace = stateMachine.transform.TransformPoint(stateMachine.characterController.center);

        Vector3 centerToBottom = new Vector3(0f, stateMachine.characterController.height * 0.5f, 0f);

        Ray downwardsRayFromCapsuleBottom = new Ray(capsuleColliderCenterInWorldSpace - centerToBottom, Vector3.down);

        if (!Physics.Raycast(downwardsRayFromCapsuleBottom, out _, groundedData.GroundToFallRayDistance, groundedData.GroundLayer, QueryTriggerInteraction.Ignore))
        {
            OnFall();
        }
    }
    protected virtual void OnFall()
    {
        stateMachine.ChangeState(stateMachine.FallingState);
    }

}