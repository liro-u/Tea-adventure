using UnityEngine;

public class PlayerInputProvider : IInputProvider<PlayerInputPayload>
{
    private readonly InputSystem input;
    private readonly Transform cameraPivot;

    private PlayerInputPayload inputPayload;
    public PlayerInputPayload InputPayload => inputPayload;

    // Latched: true from the moment the key is pressed until the next Tick() consumes it.
    // Guarantees a discrete press is never missed even if it happens between two ticks.
    private bool _jumpLatched;
    private bool _walkToggleLatched;

    public PlayerInputProvider(Transform cameraPivot)
    {
        this.cameraPivot = cameraPivot;

        input = new InputSystem();
        input.Player.Jump.started += _ => _jumpLatched = true;
        input.Player.WalkToggle.started += _ => _walkToggleLatched = true;
        input.Player.Enable();
    }

    public void Tick(float tickDelta)
    {
        // Continuous values — poll every tick
        inputPayload.MoveInput = input.Player.Move.ReadValue<Vector2>();
        inputPayload.LookInput = input.Player.Look.ReadValue<Vector2>();
        inputPayload.IsSprinting = input.Player.Sprint.IsPressed();
        inputPayload.CameraPivot = cameraPivot.rotation;

        // Discrete presses — consume the latch so each press registers for exactly one tick
        inputPayload.IsJumping = _jumpLatched;
        _jumpLatched = false;

        inputPayload.IsWalkToggle = _walkToggleLatched;
        _walkToggleLatched = false;
    }
}
