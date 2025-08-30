using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Prefab")]
    [SerializeField] private GameObject prefabToSpawn;

    [Header("Limits")]
    [SerializeField] private int maxCount = 10;
    [SerializeField] private bool maintainCount = true;
    [SerializeField] private float checkIntervalSeconds = 1.0f;

    [Header("Area (Box around this transform)")]
    [SerializeField] private Vector3 boxExtents = new Vector3(50f, 0f, 50f);

    [Header("Ground Placement (optional)")]
    [SerializeField] private bool projectToGround = true;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundOffsetY = 0.2f; // raise a bit above the ground

    [Header("Parenting & Spacing")]
    [SerializeField] private bool parentSpawnedUnderThis = true;
    [SerializeField] private float minDistanceBetween = 0f;
    [SerializeField] private int maxPlacementAttempts = 20;

    private float nextCheckTime;
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        FillToMax();
        nextCheckTime = Time.time + checkIntervalSeconds;
    }

    void Update()
    {
        if (!maintainCount || prefabToSpawn == null || maxCount <= 0)
        {
            return;
        }
        if (Time.time < nextCheckTime)
        {
            return;
        }
        nextCheckTime = Time.time + checkIntervalSeconds;
        CleanupList();
        FillToMax();
    }

    private void FillToMax()
    {
        if (prefabToSpawn == null || maxCount <= 0)
        {
            return;
        }

        CleanupList();
        int toSpawn = Mathf.Clamp(maxCount - spawnedObjects.Count, 0, maxCount);
        for (int i = 0; i < toSpawn; i++)
        {
            if (TryGetSpawnPosition(out Vector3 spawnPos, out Quaternion spawnRot))
            {
                GameObject obj = Instantiate(prefabToSpawn, spawnPos, spawnRot);
                if (parentSpawnedUnderThis)
                {
                    obj.transform.SetParent(transform, true);
                }
                spawnedObjects.Add(obj);
            }
            else
            {
                break;
            }
        }
    }

    private void CleanupList()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    private bool TryGetSpawnPosition(out Vector3 position, out Quaternion rotation)
    {
        for (int attempt = 0; attempt < Mathf.Max(1, maxPlacementAttempts); attempt++)
        {
            Vector3 randomLocal = new Vector3(
                Random.Range(-boxExtents.x, boxExtents.x),
                Random.Range(-boxExtents.y, boxExtents.y),
                Random.Range(-boxExtents.z, boxExtents.z)
            );
            Vector3 candidate = transform.position + randomLocal;

            if (projectToGround)
            {
                Vector3 rayStart = new Vector3(candidate.x, candidate.y + Mathf.Abs(raycastHeight), candidate.z);
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Abs(raycastHeight) * 2f, groundLayers))
                {
                    candidate = hit.point;
                    candidate.y += groundOffsetY;
                }
                else
                {
                    continue;
                }
            }

            if (minDistanceBetween > 0f)
            {
                bool tooClose = false;
                for (int i = 0; i < spawnedObjects.Count; i++)
                {
                    GameObject obj = spawnedObjects[i];
                    if (obj == null) continue;
                    if ((obj.transform.position - candidate).sqrMagnitude < (minDistanceBetween * minDistanceBetween))
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose)
                {
                    continue;
                }
            }

            position = candidate;
            rotation = Quaternion.identity;
            return true;
        }

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }
}

 
