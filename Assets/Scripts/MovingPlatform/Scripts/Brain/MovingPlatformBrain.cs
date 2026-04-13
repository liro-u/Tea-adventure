using UnityEngine;

/// <summary>
/// MonoBehaviour wrapper around MovingPlatformBrainCore for offline (non-networked) play.
/// Owns all serialised Unity references, constructs the core in Awake,
/// then registers/unregisters with WorldSimulation.
///
/// Fixed tick flow:
///   WorldSimulation → Core (ISimulatableEntity) → Core.OnSimulateTick(dt)
/// </summary>
public class MovingPlatformBrain : MonoBehaviour
{
    [SerializeField] private MovingPlatformSO platformData;
    [SerializeField] private EventZone        eventZone;

    [Tooltip("Ordered list of positions the platform visits. The platform starts at its scene position " +
             "and moves toward waypoints[0] when triggered.")]
    [SerializeField] private Transform[] waypoints;

    public MovingPlatformBrainCore Core { get; private set; }

    private void Awake()
    {
        if (platformData == null)                    { Debug.LogError("MovingPlatformSO not assigned.", this); return; }
        if (eventZone    == null)                    { Debug.LogError("EventZone not assigned.",         this); return; }
        if (waypoints    == null || waypoints.Length == 0) { Debug.LogError("No waypoints assigned.",   this); return; }

        Core = new MovingPlatformBrainCore(transform, platformData, waypoints, eventZone);
    }

    private void Start()     { if (Core != null) Core.Register(WorldSimulation.Instance); }
    private void OnDestroy() { if (Core != null) Core.Unregister(WorldSimulation.Instance); }
}
