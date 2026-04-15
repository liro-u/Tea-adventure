using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// NetworkBehaviour wrapper for a moving platform in online play.
///
/// This class handles only NGO concerns: spawn lifecycle, RPCs, and registration.
/// All simulation, buffering, and reconciliation logic lives in
/// NetworkMovingPlatformBrainCore / MovingPlatformBrainCore.
///
/// Registration model:
///   Server  — registers with WorldSimulation (simulation only). Sends a state
///             correction to all clients every tick via ClientRpc.
///   Clients — register with NetworkWorldSimulation for both simulation and
///             reconciliation. Simulate locally each tick using their own
///             EventZone, and reconcile when the server correction diverges.
///
/// No ServerRpc is needed: the server generates platform input from its own
/// server-side EventZone; client inputs are not sent to the server.
/// </summary>
public class NetworkMovingPlatformBrain : NetworkBehaviour, IPlatformVelocityProvider
{
    [SerializeField] private MovingPlatformSO platformData;
    [SerializeField] private EventZone        eventZone;
    [SerializeField] private SplineContainer  splinePath;

    private NetworkMovingPlatformBrainCore core;

    public Vector3 PlatformVelocity => core?.Motor.Velocity ?? Vector3.zero;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (platformData == null)         { Debug.LogError("MovingPlatformSO not assigned.",  this); return; }
        if (eventZone    == null)         { Debug.LogError("EventZone not assigned.",         this); return; }
        if (splinePath   == null)         { Debug.LogError("SplineContainer not assigned.",   this); return; }
        if (splinePath.Spline.Count < 2)  { Debug.LogError("Spline needs at least 2 knots.", this); return; }

        core = new NetworkMovingPlatformBrainCore(transform, platformData, splinePath, eventZone)
        {
            CheckDivergence = (server, local) =>
                Vector3.Distance(server.Position, local.Position) > 0.01f ||
                server.TargetKnotIndex   != local.TargetKnotIndex           ||
                server.StateId           != local.StateId                   ||
                server.WaypointDirection != local.WaypointDirection         ||
                Mathf.Abs(server.WaitTimer - local.WaitTimer) > Time.fixedDeltaTime
        };
    }

    // ── NGO lifecycle ─────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        if (core == null) return;

        if (IsServer)
        {
            // Server is authoritative: simulate and broadcast corrections every tick.
            core.OnSendStateCorrection = (state, tick) =>
                ReceiveStateCorrectionClientRpc(state, tick);
            core.InitializeTick(NetworkWorldSimulation.Instance.CurrentTick);
            core.Register(WorldSimulation.Instance);
        }
        else
        {
            // Clients simulate locally and reconcile against server corrections.
            core.RegisterWithReconciliation(NetworkWorldSimulation.Instance);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (core == null) return;

        if (IsServer)
            core.Unregister(WorldSimulation.Instance);
        else
            core.UnregisterWithReconciliation(NetworkWorldSimulation.Instance);
    }

    // ── RPCs ──────────────────────────────────────────────────────────────────

    [ClientRpc]
    private void ReceiveStateCorrectionClientRpc(MovingPlatformStateSnapshot state, int tick)
    {
        // Host (IsServer) is already authoritative; skip.
        if (IsServer) return;
        core?.ReceiveCorrection(state, tick);
    }
}
