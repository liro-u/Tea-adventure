using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// MonoBehaviour wrapper around MovingPlatformBrainCore for offline (non-networked) play.
/// Owns all serialised Unity references, constructs the core in Awake,
/// then registers/unregisters with WorldSimulation.
///
/// Fixed tick flow:
///   WorldSimulation → Core (ISimulatableEntity) → Core.OnSimulateTick(dt)
///
/// Setup: assign a SplineContainer that defines the platform's path.
/// The SplineContainer can live on any static GameObject — it must NOT be a child
/// of this platform (it would move with the platform). The platform will snap to
/// the first knot on Awake and follow the spline when triggered.
/// </summary>
public class MovingPlatformBrain : MonoBehaviour, IPlatformVelocityProvider
{
    [SerializeField] private MovingPlatformSO  platformData;
    [SerializeField] private EventZone         eventZone;
    [SerializeField] private SplineContainer   splinePath;

    public MovingPlatformBrainCore Core { get; private set; }

    public Vector3 PlatformVelocity => Core?.Motor.Velocity ?? Vector3.zero;

    private void Awake()
    {
        if (platformData == null)                          { Debug.LogError("MovingPlatformSO not assigned.",          this); return; }
        if (eventZone    == null)                          { Debug.LogError("EventZone not assigned.",                 this); return; }
        if (splinePath   == null)                          { Debug.LogError("SplineContainer not assigned.",           this); return; }
        if (splinePath.Spline.Count < 2)                  { Debug.LogError("Spline needs at least 2 knots.",          this); return; }

        Core = new MovingPlatformBrainCore(transform, platformData, splinePath, eventZone);
    }

    private void Start()     { if (Core != null) Core.Register(WorldSimulation.Instance); }
    private void OnDestroy() { if (Core != null) Core.Unregister(WorldSimulation.Instance); }
}
