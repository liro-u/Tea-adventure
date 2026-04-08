using UnityEngine;
using System;

public class CharacterAnimatorController
{
    protected Animator animator;
    protected IMovementBrain movementBrain;
    protected float rotationSmoothTime;
    protected Transform meshTransform;

    private float rotationVelocity;

    public CharacterAnimatorController(Animator animator, IMovementBrain movementBrain, float rotationSmoothTime, Transform meshTransform)
    {
        this.animator = animator;
        this.movementBrain = movementBrain;

        this.rotationSmoothTime = rotationSmoothTime;
        this.meshTransform = meshTransform;
    }

    public void Tick(float deltaTime)
    {
        HandleAnimation();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (!movementBrain.movementBrainStatePayload.IsMoving) return;

        float targetAngle = Mathf.Atan2(movementBrain.movementMotor.Velocity.x, movementBrain.movementMotor.Velocity.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        meshTransform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool("grounded", movementBrain.movementBrainStatePayload.IsGrounded);

        animator.SetBool("moving", movementBrain.movementBrainStatePayload.IsMoving);
        animator.SetBool("shouldWalk", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.WalkingState);
        animator.SetBool("shouldSprint", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.SprintingState);

        animator.SetBool("stopping", movementBrain.movementBrainStatePayload.IsStopping);
        animator.SetBool("lightStopping", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.LightStoppingState);
        animator.SetBool("hardStopping", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.HardStoppingState);

        animator.SetBool("landing", movementBrain.movementBrainStatePayload.IsLanding);
        animator.SetBool("hardLanding", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.HardLandingState);
        animator.SetBool("rolling", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.RollingState);

        animator.SetBool("jumping", movementBrain.movementStateMachine.CurrentState == movementBrain.movementStateMachine.JumpingState);
    }
}
