using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementStateMachine))]
[RequireComponent(typeof(CharacterCamera))]
public class MovementInput : NetworkBehaviour
{
    private InputSystem input;

    private MovementStateMachine characterMovementStateMachine;
    private CharacterCamera characterCamera;


    private void Awake()
    {
        input = new InputSystem();

        characterMovementStateMachine = GetComponent<MovementStateMachine>();
        characterCamera = GetComponent<CharacterCamera>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Movement input
        input.Player.Move.performed += OnMovementInput;
        input.Player.Move.canceled += OnMovementInput;

        // Look input
        input.Player.Sprint.started += OnSprintInput;
        //input.Player.Sprint.canceled += OnSprintInput;

        // Look input
        input.Player.Look.performed += OnLookInput;
        input.Player.Look.canceled += OnLookInput;

        input.Player.Jump.started += OnJumpInput;
        //input.Player.Jump.canceled += OnJumpInput;

        input.Player.Enable();
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        characterMovementStateMachine.RawMovementInputPayload.MoveInput = context.ReadValue<Vector2>();
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        characterMovementStateMachine.RawMovementInputPayload.IsRunning = context.ReadValueAsButton();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        characterMovementStateMachine.RawMovementInputPayload.IsJumping = context.ReadValueAsButton();
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        characterCamera.SetLookInput(context.ReadValue<Vector2>());
    }

    private void Update()
    {
        characterMovementStateMachine.RawMovementInputPayload.CameraPivot = characterCamera.cameraPivot.transform.rotation;
    }

    private void OnDisable()
    {
        if (IsOwner)
            input.Player.Disable();
    }
}
