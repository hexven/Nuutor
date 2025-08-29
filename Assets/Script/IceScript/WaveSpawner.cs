using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Required for UI Text

public class WaveSpawner : MonoBehaviour
{
    public List<Enemy> enemies = new List<Enemy>(); // Initialize the list
    public int currentWave = 1; // Start at Wave 1
    public int waveValue;
    public int maxEnemiesPerWave = 10; // Limit the number of enemies per wave
    public List<GameObject> enemiesToSpawn = new List<GameObject>();
    public List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies
    public Transform spawnLocation;
    public Transform player; // Reference to the player's Transform
    public int waveDuration = 10;
    public Text waveText; // Reference to UI Text for wave display
    private float waveTimer;
    private float spawnInterval;
    private float spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateWave();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if all enemies are destroyed and no enemies left to spawn
        if (enemiesToSpawn.Count == 0 && activeEnemies.Count == 0 && waveTimer <= 0)
        {
            currentWave++;
            GenerateWave();
        }

        if (spawnTimer <= 0 && enemiesToSpawn.Count > 0)
        {
            // Spawn an enemy
            GameObject enemy = Instantiate(enemiesToSpawn[0], spawnLocation.position, Quaternion.identity);
            // Track the spawned enemy
            activeEnemies.Add(enemy);
            // Get the EnemyMovement component and set the player target
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            if (enemyMovement != null && player != null)
            {
                enemyMovement.SetTarget(player);
            }
            else
            {
                Debug.LogWarning("EnemyMovement component or player reference missing on spawned enemy!");
            }
            enemiesToSpawn.RemoveAt(0); // Remove it
            spawnTimer = spawnInterval;
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
            waveTimer -= Time.fixedDeltaTime;
        }

        // Clean up destroyed enemies from activeEnemies list
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    public void GenerateWave()
    {
        // Display wave number
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave}";
            // Hide text after a few seconds
            Invoke(nameof(ClearWaveText), 3f);
        }
        else
        {
            Debug.LogWarning("Wave Text UI element not assigned!");
        }

        waveValue = currentWave * 10; // Set waveValue based on currentWave
        GenerateEnemies();

        // Avoid division by zero
        if (enemiesToSpawn.Count > 0)
        {
            spawnInterval = waveDuration / (float)enemiesToSpawn.Count;
            waveTimer = waveDuration;
        }
        else
        {
            spawnInterval = 0;
            waveTimer = 0;
        }
    }

    private void ClearWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "";
        }
    }

    public void GenerateEnemies()
    {
        List<GameObject> generatedEnemies = new List<GameObject>();
        if (enemies.Count == 0)
        {
            Debug.LogWarning("No enemies assigned to WaveSpawner!");
            return;
        }

        int enemyCount = 0; // Track number of enemies generated
        while (waveValue > 0 && enemyCount < maxEnemiesPerWave)
        {
            int randEnemy = Random.Range(0, enemies.Count);
            int randEnemyCost = enemies[randEnemy].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemy].enemyPrefab);
                waveValue -= randEnemyCost;
                enemyCount++;
            }
            else
            {
                break; // Exit loop if waveValue is too low
            }
        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
    }
}

[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int cost;
}