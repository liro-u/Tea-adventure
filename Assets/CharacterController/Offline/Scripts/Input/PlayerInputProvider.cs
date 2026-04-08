using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputProvider : IInputProvider<IMovementInputPayload>
{
    private InputSystem input;

    protected Transform cameraPivot;

    protected MovementInputPayload inputPayload;
    public IMovementInputPayload InputPayload => inputPayload;

    public PlayerInputProvider(Transform cameraPivot)
    {
        this.cameraPivot = cameraPivot;

        input = new InputSystem();

        // Movement input
        input.Player.Move.performed += OnMovementInput;
        input.Player.Move.canceled += OnMovementInput;

        input.Player.Sprint.started += OnSprintInput;
        input.Player.Sprint.canceled += OnSprintInput;

        input.Player.WalkToggle.started += OnWalkInput;
        input.Player.WalkToggle.canceled += OnWalkInput;

        // Look input
        input.Player.Look.performed += OnLookInput;
        input.Player.Look.canceled += OnLookInput;

        input.Player.Jump.started += OnJumpInput;
        input.Player.Jump.canceled += OnJumpInput;

        input.Player.Enable();
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        inputPayload.MoveInput = context.ReadValue<Vector2>();
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        inputPayload.IsSprinting = context.ReadValueAsButton();
    }

    private void OnWalkInput(InputAction.CallbackContext context)
    {
        inputPayload.IsWalkToggle = context.ReadValueAsButton();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        inputPayload.IsJumping = context.ReadValueAsButton();
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        inputPayload.LookInput = context.ReadValue<Vector2>();
    }

    public void Tick(float tickDelta)
    {
        inputPayload.CameraPivot = cameraPivot.rotation;
    }
}
