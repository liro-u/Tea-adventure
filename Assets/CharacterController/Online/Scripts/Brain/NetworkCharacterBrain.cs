using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network wrapper around the offline movement simulation.
///
/// Implements IMovementBrain directly — owns the same simulation objects as CharacterBrain
/// (motor, state machine, etc.) so all movement logic is shared without duplication.
///
/// Input flow:
///   Owner  → LocalPlayerInputProvider polls input → captured as NetworkMovementInputPayload
///           → injected into remoteInput via InjectInput() before each simulation step.
///   Others → input arrives from the network → injected the same way.
///
/// This means movementInputProvider (= remoteInput) is always the simulation source,
/// guaranteeing client predict, server processing, and reconciliation replay are identical.
/// </summary>
public class NetworkCharacterBrain : NetworkBehaviour, IMovementBrain
{
    [SerializeField] public CharacterController characterController;
    [SerializeField] private MovementSO Data;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;

    [SerializeField] public Animator animator;
    [SerializeField] Transform meshTransform;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [SerializeField] public Transform cameraPivot;
    [SerializeField] private GameObject tpsCamera;
    [SerializeField] public float sensitivity = 2.5f;
    [SerializeField] public float minPitch = -40f;
    [SerializeField] public float maxPitch = 80f;
    [SerializeField] public float smoothTime = 0.05f;

    // ── IMovementBrain ───────────────────────────────────────────────────────

    public MovementSO movementData => Data;
    public IMovementBrainStatePayload movementBrainStatePayload { get; set; }

    /// <summary>
    /// Always points to remoteInput — the single simulation input source.
    /// For owners, InjectInput() pushes captured local input here before each tick.
    /// </summary>
    public IInputProvider<IMovementInputPayload> movementInputProvider => remoteInput;

    public IAdvancedMotor movementMotor { get; private set; }
    public CharacterCamera characterCamera { get; private set; }
    public MovementStateMachine movementStateMachine { get; private set; }
    public MovementAnimationEventTrigger MovementAnimationEventTrigger => movementAnimationEventTrigger;
    public CharacterAnimatorController characterAnimatorController { get; private set; }

    // ── Network-specific ─────────────────────────────────────────────────────

    /// <summary>The single simulation input source — always used by the state machine.</summary>
    internal RemotePlayerInputProvider remoteInput { get; private set; }

    /// <summary>Owner-only: polled for local device input, then captured as a network payload.</summary>
    private LocalPlayerInputProvider localInput;

    private MovementClientPrediction prediction;

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        movementBrainStatePayload = new NetworkMovementBrainStatePayload();

        movementMotor = new AdvancedCharacterControllerMotor(
            characterController,
            Data.GroundedData.GroundToFallRayDistance,
            Data.GroundedData.StickToGroundRayDistance,
            Data.GroundedData.GroundLayer,
            1,
            Data.AirborneData.Gravity.y);

        remoteInput = new RemotePlayerInputProvider();

        movementStateMachine = new NetworkMovementStateMachine(this);

        characterCamera = new CharacterCamera(this, cameraPivot, sensitivity, minPitch, maxPitch, smoothTime);
        characterAnimatorController = new CharacterAnimatorController(animator, this, rotationSmoothTime, meshTransform);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            localInput = new LocalPlayerInputProvider(cameraPivot);
        else
            tpsCamera.SetActive(false);

        prediction = new MovementClientPrediction(this);
    }

    public override void OnNetworkDespawn()
    {
        prediction?.Dispose();
        prediction = null;
    }

    private void LateUpdate()
    {
        if (prediction == null) return;

        // Animate all players (visible to everyone).
        characterAnimatorController.Tick(Time.deltaTime);

        // Camera only for the local owner.
        if (IsOwner)
            characterCamera.Tick(Time.deltaTime);
    }

    // ── Called by MovementClientPrediction ───────────────────────────────────

    /// <summary>Polls the local input device. Only valid on the owner.</summary>
    internal void PollLocalInput(float delta) => localInput.Tick(delta);

    /// <summary>Returns the last polled local input payload. Only valid on the owner.</summary>
    internal IMovementInputPayload LocalInputPayload => localInput.InputPayload;

    /// <summary>
    /// Pushes <paramref name="input"/> into remoteInput so the state machine reads it
    /// on the next simulation step.
    /// </summary>
    internal void InjectInput(NetworkMovementInputPayload input) => remoteInput.SetPayload(input);

    // ── RPCs ──────────────────────────────────────────────────────────────────

    [ServerRpc]
    public void SendInputServerRpc(NetworkMovementInputPayload input)
        => prediction?.ReceiveInputOnServer(input);

    [ClientRpc]
    public void BroadcastStateClientRpc(NetworkMovementBrainStatePayload state)
        => prediction?.ReceiveStateOnClient(state);

    [ClientRpc]
    public void ForwardInputClientRpc(NetworkMovementInputPayload input)
        => prediction?.ReceiveForwardedInput(input);
}
