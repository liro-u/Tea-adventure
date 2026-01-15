using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerZone : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] public UnityEvent<Collider> OnEnter;

    private void Reset()
    {
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        OnEnter?.Invoke(other);
    }
}
