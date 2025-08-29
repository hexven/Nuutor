using UnityEngine;
using System.Collections.Generic;

public class Ammospawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private bool useChildrenAsSpawnPoints = true;
    [SerializeField] private Transform[] explicitSpawnPoints;
    [Tooltip("If -1, spawns at half of the available points.")]
    [SerializeField] private int maxSpawnCount = -1;
    [SerializeField] private bool randomizeOrder = true;
    [SerializeField] private bool parentSpawnedUnderThis = false;

    [Header("Random Seed (optional)")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int randomSeed = 12345;

    void Start()
    {
        if (prefabToSpawn == null)
        {
            return;
        }

        List<Transform> points = CollectSpawnPoints();
        if (points.Count == 0)
        {
            return;
        }

        if (useFixedSeed)
        {
            Random.InitState(randomSeed);
        }

        if (randomizeOrder)
        {
            Shuffle(points);
        }

        int targetSpawnCount = maxSpawnCount >= 0 ? Mathf.Min(maxSpawnCount, points.Count) : points.Count / 2;
        targetSpawnCount = Mathf.Clamp(targetSpawnCount, 0, points.Count);

        for (int i = 0; i < targetSpawnCount; i++)
        {
            Transform p = points[i];
            GameObject spawned = Instantiate(prefabToSpawn, p.position, p.rotation);
            if (parentSpawnedUnderThis && spawned != null)
            {
                spawned.transform.SetParent(transform, true);
            }
        }
    }

    private List<Transform> CollectSpawnPoints()
    {
        List<Transform> points = new List<Transform>();
        if (useChildrenAsSpawnPoints)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    points.Add(child);
                }
            }
        }

        if (explicitSpawnPoints != null && explicitSpawnPoints.Length > 0)
        {
            foreach (Transform t in explicitSpawnPoints)
            {
                if (t != null && !points.Contains(t))
                {
                    points.Add(t);
                }
            }
        }
        return points;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
