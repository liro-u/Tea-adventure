using UnityEngine;

public class MoveForwardFromSpawner : MonoBehaviour
{
    private Vector3 spawnPosition;

    private Vector3 spawnForward;

    private bool initIsDone = false;

    private void Start()
    {
        if (initIsDone) return;
        Init();
    }

    public void Init()
    {
        initIsDone = true;
        spawnPosition = transform.position;
        spawnForward = StartSpawnerManager.Instance.ForwardDirection.normalized;
    }


    private void Update()
    {
        // Move forward
        Move(Time.deltaTime);
    }

    public void Move(float dt)
    {
        if (StartSpawnerManager.Instance == null)
            return;

        transform.position += spawnForward * StartSpawnerManager.Instance.forwardMoveSpeed * dt;

        // Compute distance along forward axis
        float distance = Vector3.Dot(transform.position - spawnPosition, spawnForward);

        if (distance >= StartSpawnerManager.Instance.maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
