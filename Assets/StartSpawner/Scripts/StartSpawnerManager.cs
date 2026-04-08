using System.Collections.Generic;
using UnityEngine;

public class StartSpawnerManager : MonoBehaviour
{
    public static StartSpawnerManager Instance { get; private set; }

    [Header("Movement Settings")]
    public float forwardMoveSpeed = 5f;
    public float maxDistance = 30f;

    [Header("Simulation Bake")]
    [Tooltip("Simulate spawns as if the spawner has already been running for this amount of time.")]
    public float bakeSimulationTime = 0f;

    [Header("Parent for spawned objects")]
    public Transform spawnParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Vector3 ForwardDirection
    {
        get
        {
            return baseSpawnTransform != null
                ? baseSpawnTransform.forward
                : Vector3.forward;
        }
    }

    #region Spawn Line Settings

    [Header("Base Spawn Transform")]
    public Transform baseSpawnTransform;

    [Header("Horizontal Spawn Area")]
    public float spawnWidth = 10f;        // total width
    public float deadZoneWidth = 2f;      // middle forbidden zone

    #endregion

    #region Automatic Spawn

    [Header("Cooldown Settings")]
    public float minCooldown = 1f;
    public float maxCooldown = 5f;

    private float timer;
    private float nextCooldown;

    void Start()
    {
        ResetCooldown();

        if (bakeSimulationTime > 0f)
        {
            float simulatedTime = 0f;

            while (simulatedTime < bakeSimulationTime)
            {
                GameObject go = Spawn();

                // Random next cooldown
                float next = Random.Range(minCooldown, maxCooldown);

                // Move the spawned object forward as if time has already passed
                if (go != null)
                {
                    var mover = go.GetComponent<MoveForwardFromSpawner>();
                    if (mover != null)
                    {
                        mover.Init();
                        mover.Move(simulatedTime);
                    }
                }

                simulatedTime += next;
            }
        }

    }


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextCooldown)
        {
            Spawn();
            ResetCooldown();
        }
    }

    private void ResetCooldown()
    {
        timer = 0f;
        nextCooldown = Random.Range(minCooldown, maxCooldown);
    }

    #endregion

    #region Spawn Logic

    [SerializeField] private List<SpawnableObject> objects = new();

    public GameObject Spawn()
    {
        if (objects.Count == 0 || baseSpawnTransform == null)
        {
            Debug.LogWarning("Spawner missing data.");
            return null;
        }

        SpawnableObject chosen = GetWeightedRandom(objects);

        Vector3 horizontalOffset = GetRandomHorizontalOffset();

        Vector3 pos =
            baseSpawnTransform.position +
            horizontalOffset +
            baseSpawnTransform.TransformDirection(chosen.basePositionOffset) +
            RandomVector(chosen.randomPositionRange);

        Vector3 rot =
            baseSpawnTransform.eulerAngles +
            chosen.baseRotationOffset +
            RandomVector(chosen.randomRotationRange);

        Vector3 randomScale = RandomVector(chosen.randomScaleRange) + Vector3.one;
        Vector3 scale =
            new Vector3(chosen.baseScale.x * randomScale.x, chosen.baseScale.y * randomScale.y, chosen.baseScale.z * randomScale.z);

        GameObject go = Instantiate(chosen.prefab, spawnParent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(rot);
        go.transform.localScale = scale;

        if (go.GetComponent<MoveForwardFromSpawner>() == null)
        {
            go.AddComponent<MoveForwardFromSpawner>();
        }

        return go;
    }

    private Vector3 GetRandomHorizontalOffset()
    {
        float halfWidth = spawnWidth * 0.5f;
        float halfDead = deadZoneWidth * 0.5f;

        bool spawnLeft = Random.value < 0.5f;

        float distance;

        if (spawnLeft)
            distance = Random.Range(-halfWidth, -halfDead);
        else
            distance = Random.Range(halfDead, halfWidth);

        // Use transform's local right axis (rotation-aware)
        return baseSpawnTransform.right * distance;
    }


    private Vector3 RandomVector(Vector3 range)
    {
        return new Vector3(
            Random.Range(-range.x, range.x),
            Random.Range(-range.y, range.y),
            Random.Range(-range.z, range.z)
        );
    }

    private SpawnableObject GetWeightedRandom(List<SpawnableObject> list)
    {
        float totalWeight = 0f;
        foreach (var obj in list)
            totalWeight += obj.weight;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (var obj in list)
        {
            randomValue -= obj.weight;
            if (randomValue <= 0f)
                return obj;
        }

        return list[^1];
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (baseSpawnTransform == null)
            return;

        Vector3 center = baseSpawnTransform.position;

        float halfWidth = spawnWidth * 0.5f;
        float halfDead = deadZoneWidth * 0.5f;

        Vector3 right = baseSpawnTransform.right;
        Vector3 forward = baseSpawnTransform.forward;

        // --- Horizontal spawn zones ---
        Vector3 leftStart = center - right * halfWidth;
        Vector3 leftEnd = center - right * halfDead;

        Vector3 rightStart = center + right * halfDead;
        Vector3 rightEnd = center + right * halfWidth;

        // Allowed zones (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftStart, leftEnd);
        Gizmos.DrawLine(rightStart, rightEnd);

        // Dead zone (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftEnd, rightStart);

        // Base point (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.1f);

        // --- Forward direction (cyan) ---
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, center + forward * maxDistance);

        // --- Void Line (red) ---
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rightStart, rightStart + forward * maxDistance);
        Gizmos.DrawLine(leftEnd, leftEnd + forward * maxDistance);


        // --- Max distance "delete line" (magenta) ---
        Vector3 deleteLineCenter = center + forward * maxDistance;
        Vector3 deleteLeft = deleteLineCenter - right * halfWidth;
        Vector3 deleteRight = deleteLineCenter + right * halfWidth;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(deleteLeft, deleteRight);
    }


    #endregion
}
