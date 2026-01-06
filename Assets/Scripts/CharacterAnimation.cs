using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    
    [Header("References")]
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private Transform meshTransform;

    [Header("Animator Params")]
    [SerializeField] private string movingBoolName = "moving";
    [SerializeField] private string runningBoolName = "running";

    public Animator animator;
    
    private float rotationVelocity;


    private void Update()
    {
        HandleRotation();
        HandleAnimation();
    }

    private void HandleRotation()
    {
        if (!characterMovement.IsMoving) return;

        if (characterMovement.MoveVelocity.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(characterMovement.MoveVelocity.x, characterMovement.MoveVelocity.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(meshTransform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        meshTransform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(movingBoolName, characterMovement.IsMoving);
        animator.SetBool(runningBoolName, characterMovement.IsRunning);
    }
}
