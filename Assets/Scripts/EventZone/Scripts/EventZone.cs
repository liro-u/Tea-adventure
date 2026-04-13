using System;
using UnityEngine;

/// <summary>
/// Trigger sensor that tracks whether one or more qualifying colliders are inside the volume.
///
/// Acts as an input source for any system that needs zone-based activation
/// (e.g. MovingPlatformInputProvider). The zone itself has no knowledge of what
/// it drives — it just exposes IsTriggered.
///
/// Replay-safe: OnTriggerEnter/Exit callbacks are suppressed while IsReplayActive
/// is true, so that reconciliation replays in the online layer never re-fire the zone.
/// The online NetworkBehaviour wrapper is responsible for setting this flag around replays.
///
/// Overlap counting: entering/exiting colliders increment/decrement a counter so that
/// IsTriggered stays true as long as at least one qualifying collider is inside.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EventZone : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayers = ~0;

    private int overlapCount;

    /// <summary>True when at least one qualifying collider is currently inside the zone.</summary>
    public bool IsTriggered => overlapCount > 0;

    /// <summary>
    /// When true, OnTriggerEnter/Exit callbacks are suppressed.
    /// Set by the online layer before replay starts; cleared when replay ends.
    /// </summary>
    public bool IsReplayActive { get; set; }

    /// <summary>Raised when the first qualifying collider enters (false → true transition).</summary>
    public event Action Activated;

    /// <summary>Raised when the last qualifying collider exits (true → false transition).</summary>
    public event Action Deactivated;

    private void OnTriggerEnter(Collider other)
    {
        if (IsReplayActive) return;
        if (!IsInTriggerLayers(other)) return;

        overlapCount++;
        if (overlapCount == 1)
            Activated?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsReplayActive) return;
        if (!IsInTriggerLayers(other)) return;

        overlapCount = Mathf.Max(0, overlapCount - 1);
        if (overlapCount == 0)
            Deactivated?.Invoke();
    }

    /// <summary>
    /// Resets overlap state to zero without firing Deactivated.
    /// Call after a simulation rewind so the zone is consistent with the restored tick.
    /// </summary>
    public void ResetOverlapState() => overlapCount = 0;

    private bool IsInTriggerLayers(Collider other) =>
        (triggerLayers.value & (1 << other.gameObject.layer)) != 0;
}
