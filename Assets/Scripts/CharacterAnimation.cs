using Unity.Netcode;
using UnityEngine;

public class CharacterAnimation : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    
    [Header("References")]
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private Transform meshTransform;

    [Header("Animator Params")]
    [SerializeField] private string movingBoolName = "moving";
    [SerializeField] private string runningBoolName = "running";

    public Animator animator { private get; set; }
    
    private float rotationVelocity;


    private void Update()
    {
        HandleRotation();
        HandleAnimation();
    }

    private void HandleRotation()
    {
        if (!characterMovement.GetCurrentState().IsMoving) return;

        float targetAngle = Mathf.Atan2(characterMovement.GetCurrentState().Displacement.x, characterMovement.GetCurrentState().Displacement.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        meshTransform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(movingBoolName, characterMovement.GetCurrentState().IsMoving);
        animator.SetBool(runningBoolName, characterMovement.GetCurrentState().IsRunning);
    }
}
