using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem.LowLevel;
using System;

public struct MovementInputPayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector2 MoveInput;
    public Quaternion CameraPivot;
    public bool IsRunning;
    public bool IsJumping;
}

public struct MovementStatePayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public MovementStateId StateId;

    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 TargetDirection;

    public float MovementSpeedModifier;
    public float MovementDecelerationForce;
    public int RemainingJump;
    public Vector3 CurrentJumpForce;

    public bool IsGrounded;
    public bool IsMoving;
    public bool IsStopping;

    public override string ToString()
    {
        return $"{Tick} : {StateId.ToString()}";
    }
}

[GenerateSerializationForType(typeof(MovementInputPayload))]
[GenerateSerializationForType(typeof(MovementStatePayload))]
public class MovementStateMachine : StateMachine<MovementInputPayload, MovementStatePayload>
{
    [SerializeField] public Transform cameraPivot;
    [SerializeField] public CharacterController characterController;
    [SerializeField] public MovementSO Data;



    public MovementInputPayload RawMovementInputPayload;
    public MovementStatePayload RawMovementStatePayload;

    public IdlingState IdlingState;

    public WalkingState WalkingState;
    public RunningState RunningState;

    public LightStoppingState LightStoppingState;
    public HardStoppingState HardStoppingState;

    public JumpingState JumpingState;
    public FallingState FallingState;

    private void Awake()
    {
        RawMovementStatePayload = new MovementStatePayload();
        RawMovementStatePayload.TargetDirection = Vector3.forward;


        IdlingState = new IdlingState(this);

        WalkingState = new WalkingState(this);
        RunningState = new RunningState(this);

        LightStoppingState = new LightStoppingState(this);
        HardStoppingState = new HardStoppingState(this);

        JumpingState = new JumpingState(this);
        FallingState = new FallingState(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ChangeState(IdlingState);
    }

    protected override bool ReconciliationNeeded(MovementStatePayload latestServerState, MovementStatePayload matchingClientState)
    {
        float error = Vector3.Distance(
            latestServerState.Position,
            matchingClientState.Position
        );
        return error > 0.001f;
    }

    protected override MovementInputPayload CreateInputPayload(int currentTick)
    {
        RawMovementInputPayload.Tick = currentTick;
        return RawMovementInputPayload;
    }

    protected override void ApplyNetworkState(MovementStatePayload state)
    {
        base.ApplyNetworkState(state);

        RawMovementStatePayload = state;
        currentState = getStateById(state.StateId);
    }

    private MovementState getStateById(MovementStateId id)
    {
        return id switch
        {
            MovementStateId.Idling => IdlingState,
            MovementStateId.Running => RunningState,
            MovementStateId.Walking => WalkingState,
            MovementStateId.LightStopping => LightStoppingState,
            MovementStateId.HardStopping => HardStoppingState,
            MovementStateId.Jumping => JumpingState,
            MovementStateId.Falling => FallingState,
            _ => throw new ArgumentOutOfRangeException(
                nameof(id),
                id,
                "Unknown MovementStateId. Network state is invalid or desynced."
            )
        };
    }


    private void LateUpdate()
    {
        Debuger.Instance.Add($"Client {OwnerClientId} : {GetCurrentNetworkState().StateId}");
        Debuger.Instance.Add($"Position : {GetCurrentNetworkState().Position}");
        Debuger.Instance.Add($"Velocity : {GetCurrentNetworkState().Velocity}");
        Debuger.Instance.Add($"CurrentJumpForce : {GetCurrentNetworkState().CurrentJumpForce}");

        characterController.enabled = false;
        transform.position = Vector3.Lerp(transform.position, GetCurrentNetworkState().Position, 10 * Time.deltaTime);
        characterController.enabled = true;
    }

}
