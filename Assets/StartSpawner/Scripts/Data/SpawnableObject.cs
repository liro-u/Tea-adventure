using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;

    [Header("Base Transform")]
    public Vector3 basePositionOffset;
    public Vector3 baseRotationOffset;
    public Vector3 baseScale = Vector3.one;

    [Header("Random Offset")]
    public Vector3 randomPositionRange;
    public Vector3 randomRotationRange;
    public Vector3 randomScaleRange;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float weight = 1f; // probability weight
}
