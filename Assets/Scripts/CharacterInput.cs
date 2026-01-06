using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterCamera))]
public class CharacterInput : MonoBehaviour
{
    private InputSystem input;

    private CharacterMovement characterMovement;
    private CharacterCamera characterCamera;


    private void Awake()
    {
        input = new InputSystem();

        characterMovement = GetComponent<CharacterMovement>();
        characterCamera = GetComponent<CharacterCamera>();


        // Movement input
        input.Player.Move.performed += OnMovementInput;
        input.Player.Move.canceled += OnMovementInput;

        // Look input
        input.Player.Sprint.performed += OnSprintInput;

        // Look input
        input.Player.Look.performed += OnLookInput;
        input.Player.Look.canceled += OnLookInput;
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        characterMovement.SetMoveInput(context.ReadValue<Vector2>());
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        characterMovement.SetIsRunning(context.ReadValueAsButton());
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        characterCamera.SetLookInput(context.ReadValue<Vector2>());
    }

    private void OnEnable() => input.Player.Enable();
    private void OnDisable() => input.Player.Disable();

}
