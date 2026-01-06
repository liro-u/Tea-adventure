using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerZone : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private UnityEvent onPlayerEnter;

    private void Reset()
    {
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("here");
        if (!other.CompareTag(targetTag))
            return;

        Debug.Log("here 2");
        onPlayerEnter?.Invoke();
    }
}
