using Unity.Netcode;
using UnityEngine;

public class MovementAnimation : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("References")]
    [SerializeField] private MovementStateMachine characterMovementStateMachine;
    [SerializeField] private Transform meshTransform;

    [Header("Animator Params")]
    [SerializeField] private string movingBoolName = "moving";
    [SerializeField] private string shouldWalkBoolName = "shouldWalk";
    [SerializeField] private string shouldSprintBoolName = "shouldSprint";
    [SerializeField] private string stoppingBoolName = "stopping";
    [SerializeField] private string lightStoppingBoolName = "lightStopping";
    [SerializeField] private string hardStoppingBoolName = "hardStopping";
    [SerializeField] private string groundedBoolName = "grounded";
    [SerializeField] private string jumpingBoolName = "jumping";
    [SerializeField] private string landingBoolName = "landing";
    [SerializeField] private string hardLandingBoolName = "hardLanding";
    [SerializeField] private string rollingBoolName = "rolling";

    [SerializeField] public Animator animator;

    private float rotationVelocity;


    private void Update()
    {
        HandleRotation();
        HandleAnimation();
    }

    private void HandleRotation()
    {
        if (!characterMovementStateMachine.GetCurrentNetworkState().IsMoving) return;

        float targetAngle = Mathf.Atan2(characterMovementStateMachine.GetCurrentNetworkState().Velocity.x, characterMovementStateMachine.GetCurrentNetworkState().Velocity.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        meshTransform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(groundedBoolName, characterMovementStateMachine.GetCurrentNetworkState().IsGrounded);

        animator.SetBool(movingBoolName, characterMovementStateMachine.GetCurrentNetworkState().IsMoving);
        animator.SetBool(shouldWalkBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.WalkingState.StateId);
        animator.SetBool(shouldSprintBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.SprintingState.StateId);

        animator.SetBool(stoppingBoolName, characterMovementStateMachine.GetCurrentNetworkState().IsStopping);
        animator.SetBool(lightStoppingBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.LightStoppingState.StateId);
        animator.SetBool(hardStoppingBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.HardStoppingState.StateId);

        animator.SetBool(landingBoolName, characterMovementStateMachine.GetCurrentNetworkState().IsLanding);
        animator.SetBool(hardLandingBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.HardLandingState.StateId);
        animator.SetBool(rollingBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.RollingState.StateId);

        animator.SetBool(jumpingBoolName, characterMovementStateMachine.GetCurrentNetworkState().StateId == characterMovementStateMachine.JumpingState.StateId);
    }
}
