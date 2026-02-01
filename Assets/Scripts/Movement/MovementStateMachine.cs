using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem.LowLevel;
using System;

public struct MovementInputPayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public Vector2 MoveInput;
    public Quaternion CameraPivot;
    public bool IsSprinting;
    public bool IsWalkToggle;
    public bool IsJumping;
}

public struct MovementStatePayload : ITickPayload, INetworkSerializeByMemcpy
{
    public int Tick { get; set; }
    public MovementStateId StateId;

    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 TargetDirection;

    public bool ShouldWalk;
    public bool ShouldSprint;

    public float MovementSpeedModifier;
    public float MovementDecelerationForce;
    public int RemainingJump;
    public Vector3 CurrentJumpForce;

    public bool IsGrounded;
    public bool IsMoving;
    public bool IsStopping;
    public bool IsLanding;

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


    public IdlingState IdlingState;

    public WalkingState WalkingState;
    public RunningState RunningState;
    public SprintingState SprintingState;

    public LightStoppingState LightStoppingState;
    public MediumStoppingState MediumStoppingState;
    public HardStoppingState HardStoppingState;

    public LightLandingState LightLandingState;
    public HardLandingState HardLandingState;
    public RollingState RollingState;

    public JumpingState JumpingState;
    public FallingState FallingState;

    private void Awake()
    {
        RawStatePayload = new MovementStatePayload();
        RawStatePayload.TargetDirection = Vector3.forward;


        IdlingState = new IdlingState(this);

        WalkingState = new WalkingState(this);
        RunningState = new RunningState(this);
        SprintingState = new SprintingState(this);

        LightStoppingState = new LightStoppingState(this);
        MediumStoppingState = new MediumStoppingState(this);
        HardStoppingState = new HardStoppingState(this);

        LightLandingState = new LightLandingState(this);
        HardLandingState = new HardLandingState(this);
        RollingState = new RollingState(this);

        JumpingState = new JumpingState(this);
        FallingState = new FallingState(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ChangeState(IdlingState);
    }

    protected override void ResetRawInputPayload()
    {
        base.ResetRawInputPayload();

        Vector2 oldMoveInput = RawInputPayload.MoveInput;
        Quaternion oldCam = RawInputPayload.CameraPivot;

        RawInputPayload = new MovementInputPayload();
        RawInputPayload.MoveInput = oldMoveInput;
        RawInputPayload.CameraPivot = oldCam;
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
        RawInputPayload.Tick = currentTick;
        return RawInputPayload;
    }

    protected override void ApplyNetworkState(MovementStatePayload state)
    {
        base.ApplyNetworkState(state);

        RawStatePayload = state;
        currentState = getStateById(state.StateId);
    }

    private MovementState getStateById(MovementStateId id)
    {
        return id switch
        {
            MovementStateId.Idling => IdlingState,

            MovementStateId.Walking => WalkingState,
            MovementStateId.Running => RunningState,
            MovementStateId.Sprinting => SprintingState,

            MovementStateId.LightStopping => LightStoppingState,
            MovementStateId.MediumStopping => MediumStoppingState,
            MovementStateId.HardStopping => HardStoppingState,

            MovementStateId.LightLanding => LightLandingState,
            MovementStateId.HardLanding => HardLandingState,
            MovementStateId.Rolling => RollingState,

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
        Debuger.Instance.Add($"Client {OwnerClientId} : {GetCurrentNetworkState().StateId} - {CurrentNetworkState.StateId} - {latestServerNetworkState.StateId}");
        Debuger.Instance.Add($"Position : {GetCurrentNetworkState().Position}");
        Debuger.Instance.Add($"Velocity : {GetCurrentNetworkState().Velocity}");
        Debuger.Instance.Add($"CurrentJumpForce : {GetCurrentNetworkState().CurrentJumpForce}");

        characterController.enabled = false;
        transform.position = Vector3.Lerp(transform.position, GetCurrentNetworkState().Position, 10 * Time.deltaTime);
        characterController.enabled = true;
    }

}
