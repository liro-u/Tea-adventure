using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterBrain : NetworkBehaviour, IMovementBrain
{
    [SerializeField] public CharacterController characterController;
    [SerializeField] private MovementSO Data;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;

    [SerializeField] public Animator animator;
    [SerializeField] Transform meshTransform;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [SerializeField] public Transform cameraPivot;
    [SerializeField] public float sensitivity = 2.5f;
    [SerializeField] public float minPitch = -40f;
    [SerializeField] public float maxPitch = 80f;
    [SerializeField] public float smoothTime = 0.05f;

    public MovementSO movementData
    {
        get => Data;
        protected set => Data = value;
    }

    public IMovementBrainStatePayload movementBrainStatePayload { get; set; }
    public IInputProvider<IMovementInputPayload> movementInputProvider { get; protected set; }

    public IAdvancedMotor movementMotor { get; protected set; }
    public CharacterCamera characterCamera { get; protected set; }
    public MovementStateMachine movementStateMachine { get; protected set; }

    public MovementAnimationEventTrigger MovementAnimationEventTrigger { get => movementAnimationEventTrigger; }
    public CharacterAnimatorController characterAnimatorController { get; protected set; }
    public MovementClientPrediction movementClientPrediction { get; protected set; }

    public void Awake()
    {
        characterAnimatorController = new CharacterAnimatorController(animator, this, rotationSmoothTime, meshTransform);

        movementBrainStatePayload = new NetworkMovementBrainStatePayload();

        movementMotor = new AdvancedCharacterControllerMotor(
            characterController,
            Data.GroundedData.GroundToFallRayDistance,
            Data.GroundedData.StickToGroundRayDistance,
            Data.GroundedData.GroundLayer,
            1,
            Data.AirborneData.Gravity.y);

        // Input provider is set in OnNetworkSpawn once ownership is known.
        movementInputProvider = new AIInputProvider();

        movementClientPrediction = new MovementClientPrediction(this);

        movementStateMachine = new NetworkMovementStateMachine(this);

        characterCamera = new CharacterCamera(this, cameraPivot, sensitivity, minPitch, maxPitch, smoothTime);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            movementInputProvider = new PlayerInputProvider(cameraPivot);
    }

    public void Update()
    {
        movementClientPrediction.Update();
    }

    private void LateUpdate()
    {
        Debuger.Instance.Clear();
        Debuger.Instance.Add($"State : {movementStateMachine.CurrentState}");
        Debuger.Instance.Add($"Position : {movementMotor.Position}");
        Debuger.Instance.Add($"Velocity : {movementMotor.Velocity}");
        Debuger.Instance.Add($"IsGrounded : {movementMotor.IsGrounded}");
    }

    public void Tick(float tickDelta)
    {
        characterAnimatorController.Tick(tickDelta);
        characterCamera.Tick(tickDelta);
        movementInputProvider.Tick(tickDelta);

        movementStateMachine.Tick(tickDelta);
        movementMotor.ApplyForce(tickDelta, movementBrainStatePayload.IsGrounded);
    }

    // ================= RPCs =================

    [ServerRpc]
    public void SendMovementInputServerRpc(NetworkMovementInputPayload input)
        => movementClientPrediction.OnReceiveInputFromClient(input);

    [ClientRpc]
    public void BroadcastMovementStateClientRpc(NetworkMovementBrainStatePayload state)
        => movementClientPrediction.OnReceiveStateFromServer(state);

    [ClientRpc]
    public void ForwardMovementInputClientRpc(NetworkMovementInputPayload input)
        => movementClientPrediction.OnReceiveForwardedInput(input);
}
