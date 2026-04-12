using Unity.Netcode;
using UnityEngine;

/// <summary>
/// NetworkBehaviour wrapper for online play.
/// Creates a NetworkCharacterBrainCore, wires its Prediction callbacks to RPCs,
/// and registers Prediction (not the core) with WorldSimulation / NetworkWorldSimulation.
///
/// All simulation and prediction logic is in CharacterBrainCore / ClientPrediction.
/// This class is responsible only for NGO concerns: spawn events, RPCs, and Update gating.
/// </summary>
public class NetworkCharacterBrain : NetworkBehaviour
{
    [SerializeField] private CharacterController           characterController;
    [SerializeField] private MovementSO                    movementSO;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;
    [SerializeField] private Animator                      animator;
    [SerializeField] private Transform                     meshTransform;
    [SerializeField] private float                         rotationSmoothTime = 0.1f;
    [SerializeField] private Transform                     cameraPivot;
    [SerializeField] private float                         sensitivity        = 2.5f;
    [SerializeField] private float                         minPitch           = -40f;
    [SerializeField] private float                         maxPitch           =  80f;
    [SerializeField] private float                         smoothTime         = 0.05f;

    private NetworkCharacterBrainCore brain;

    // ── Unity / NGO lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        if (characterController == null) { Debug.LogError("CharacterController not assigned.", this); return; }
        if (movementSO          == null) { Debug.LogError("MovementSO not assigned.",          this); return; }
        if (animator            == null) { Debug.LogError("Animator not assigned.",            this); return; }
        if (cameraPivot         == null) { Debug.LogError("CameraPivot not assigned.",         this); return; }
        if (meshTransform       == null) { Debug.LogError("MeshTransform not assigned.",       this); return; }

        brain = new NetworkCharacterBrainCore(
            this,
            characterController, movementSO, animator, meshTransform,
            movementAnimationEventTrigger, cameraPivot,
            sensitivity, minPitch, maxPitch, smoothTime, rotationSmoothTime);

        brain.Prediction.OnSendInput = (input, tick, prevInput, prevTick) =>
            SubmitInputServerRpc(input, tick, prevInput, prevTick);

        brain.Prediction.OnSendStateCorrection = (state, tick) =>
            ReceiveStateCorrectionClientRpc(state, tick, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
            });
    }

    public override void OnNetworkSpawn()
    {
        if (brain == null) return;
        if (IsOwner && !IsServer)
            brain.Prediction.RegisterWithReconciliation(NetworkWorldSimulation.Instance);
        else if (IsServer)
            brain.Prediction.Register(WorldSimulation.Instance);
    }

    public override void OnNetworkDespawn()
    {
        if (brain == null) return;
        if (IsOwner && !IsServer)
            brain.Prediction.UnregisterWithReconciliation(NetworkWorldSimulation.Instance);
        else if (IsServer)
            brain.Prediction.Unregister(WorldSimulation.Instance);
    }

    private void Update()
    {
        if (brain == null) return;
        if (IsOwner)
            brain.OnUpdate(Time.deltaTime);
        else
            brain.characterAnimatorController.Tick(Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (!IsOwner || brain == null) return;
        Debuger.Instance.Clear();
        Debuger.Instance.Add($"State    : {brain.movementStateMachine.CurrentState}");
        Debuger.Instance.Add($"Position : {brain.movementMotor.Position}");
        Debuger.Instance.Add($"Velocity : {brain.movementMotor.Velocity}");
        Debuger.Instance.Add($"Grounded : {brain.movementMotor.IsGrounded}");
    }

    // ── RPCs ──────────────────────────────────────────────────────────────────

    [ServerRpc]
    private void SubmitInputServerRpc(PlayerInputPayload input, int tick,
        PlayerInputPayload prevInput, int prevTick,
        ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
        brain.Prediction.EnqueueServerInput(input, tick);
        brain.Prediction.EnqueueServerInput(prevInput, prevTick);
    }

    [ClientRpc]
    private void ReceiveStateCorrectionClientRpc(PlayerStateSnapshot state, int tick,
        ClientRpcParams rpcParams = default)
    {
        if (IsOwner)
            brain.Prediction.ReceiveCorrection(state, tick);
        else
            brain.ApplyState(state);
    }
}
