using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private Transform cameraPivot;

    private CharacterController characterController;

    private Vector2 moveInput;
    private Vector3 currentMovement;
    private bool isMoving;
    private bool isRunning;

    public Vector3 MoveVelocity => currentMovement; // expose to animation
    public bool IsMoving => isMoving;     // expose to animation
    public bool IsRunning => isRunning;     // expose to animation

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        isMoving = moveInput.sqrMagnitude > 0;
        if (!isMoving) isRunning = false;
    }

    public void SetIsRunning(bool input)
    {
        if (isMoving)
        {
            isRunning = input;
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Camera-relative rotation (yaw only)
        Vector3 cameraForward = cameraPivot.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraPivot.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraRelativeMove =
            cameraForward * move.z +
            cameraRight * move.x;

        float speed = isRunning ? runSpeed : walkSpeed;
        currentMovement = cameraRelativeMove * speed;
        characterController.SimpleMove(currentMovement);
    }
}
