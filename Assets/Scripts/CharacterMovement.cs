using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.Windows;
using UnityEngine.Splines;

public struct MoveInputPayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector2 MoveInput;
    public Quaternion CameraPivot;
    public bool Run;
}

public struct MovementStatePayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector3 Position;
    public Vector3 Displacement;

    public float VerticalVelocity;
    public bool IsGrounded;
    public bool IsRunning;
    public bool IsMoving;
}


[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : ClientPredictionNetworkBehaviour<MoveInputPayload, MovementStatePayload>
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 4f;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedStickForce = -2f;

    [SerializeField] private Transform cameraPivot;

    private CharacterController characterController;

    private Vector2 moveInput;
    public bool IsMoving { get; private set; }
    public bool IsRunning { get; private set; }

    private float interpolationSpeed = 10f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        IsMoving = moveInput.sqrMagnitude > 0;
        if (!IsMoving) IsRunning = false;
    }

    public void SetIsRunning(bool input)
    {
        if (IsMoving)
        {
            IsRunning = input;
        }
    }

    private void LateUpdate()
    {
        characterController.enabled = false;
        transform.position = Vector3.Lerp(transform.position, GetCurrentState().Position , interpolationSpeed * Time.deltaTime);
        characterController.enabled = true;
    }


    protected override bool ReconciliationNeeded(MovementStatePayload latestServerState, MovementStatePayload matchingClientState)
    {
        float error = Vector3.Distance(
            latestServerState.Position,
            matchingClientState.Position
        );
        return error > 0.001f;
    }

    protected override MoveInputPayload CreateInputPayload(int currentTick)
    {
        return new MoveInputPayload
        {
            Tick = currentTick,
            MoveInput = moveInput,
            CameraPivot = cameraPivot.rotation,
            Run = IsRunning,
        };
    }

    protected override MovementStatePayload Simulate(MoveInputPayload input)
    {

        int prevIndex = (input.Tick - 1 + BUFFER_SIZE) % BUFFER_SIZE;
        MovementStatePayload prevState = stateBuffer[prevIndex];

        Vector3 currentPosition = transform.position;
        characterController.enabled = false;
        transform.position = prevState.Position;
        characterController.enabled = true;

        // Horizontal movement
        Vector3 move = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // Extract camera yaw
        Quaternion cameraYaw = Quaternion.Euler(0f, input.CameraPivot.eulerAngles.y, 0f);

        // Rotate movement by camera yaw
        Vector3 worldMove = cameraYaw * move;

        float speed = input.Run ? runSpeed : walkSpeed;
        Vector3 horizontalVelocity = worldMove * speed;

        // Gravity (deterministic)
        float verticalVelocity = prevState.VerticalVelocity;

        if (prevState.IsGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }
        else
        {
            verticalVelocity += gravity * tickDelta;
        }

        Vector3 velocity = new Vector3(
            horizontalVelocity.x,
            verticalVelocity,
            horizontalVelocity.z
        );

        Vector3 displacement = velocity * tickDelta;

        characterController.Move(displacement);
        Vector3 nextPosition = transform.position;
        bool isGrounded = characterController.isGrounded;

        characterController.enabled = false;
        transform.position = currentPosition;
        characterController.enabled = true;

        return new MovementStatePayload
        {
            Tick = input.Tick,
            Position = nextPosition,
            Displacement = displacement,
            VerticalVelocity = verticalVelocity,
            IsGrounded = isGrounded,
            IsRunning = input.Run,
            IsMoving = input.MoveInput.sqrMagnitude > 0
        };
    }
}
