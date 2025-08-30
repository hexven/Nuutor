using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Required for UI Text and CanvasGroup
using System.Collections; // Required for coroutines

public class WaveSpawner : MonoBehaviour
{
    public List<Enemy> enemies = new List<Enemy>(); // Initialize the list
    public int currentWave = 1; // Start at Wave 1
    public int maxWaves = 5; // Maximum number of waves in the game
    public int waveValue;
    public List<int> maxEnemiesPerWave = new List<int> { 10, 15, 20, 25, 30 }; // Per-wave enemy limits
    public List<GameObject> enemiesToSpawn = new List<GameObject>();
    public List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies
    public List<Transform> spawnLocations = new List<Transform>(); // List of spawn locations
    public Transform player; // Reference to the player's Transform
    public int waveDuration = 10;
    public Text waveText; // Reference to UI Text for wave display
    public CanvasGroup waveTextCanvasGroup; // CanvasGroup for fading wave text
    public Text enemyCountText; // Reference to UI Text for enemy count display
    public bool facePlayerOnSpawn = true; // Option to face player on spawn
    public Vector3 customFacingDirection = Vector3.right; // Custom direction if not facing player
    public float fadeDuration = 2f; // Duration of fade-out animation in seconds
    public float spawnRadius = 1f; // NEW: Minimum distance between spawned enemies
    public float maxSpawnOffset = 0.5f; // NEW: Maximum offset to try if spawn position is occupied
    private float waveTimer;
    private float spawnInterval;
    private float spawnTimer;
    private int maxEnemiesThisWave; // Track max enemies for the current wave
    private int destroyedEnemies; // Track number of enemies destroyed in the current wave

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateWave();
    }

    // Update is called once per frame
    void Update()
    {
        // Update UI text for wave and enemy count separately
        if (waveText != null)
        {
            if (currentWave > maxWaves && activeEnemies.Count == 0)
            {
                waveText.text = "Game Complete!";
                if (waveTextCanvasGroup != null)
                {
                    waveTextCanvasGroup.alpha = 1f; // Ensure visible for game completion
                }
            }
            // Wave text is set in GenerateWave for active waves
        }
        else
        {
            Debug.LogWarning("Wave Text UI element not assigned!");
        }

        if (enemyCountText != null)
        {
            if (currentWave <= maxWaves)
            {
                enemyCountText.text = $": {destroyedEnemies}/{maxEnemiesThisWave}";
                Debug.Log($"Updating enemy count: {destroyedEnemies}/{maxEnemiesThisWave} (Active: {activeEnemies.Count}, ToSpawn: {enemiesToSpawn.Count})");
            }
            else if (activeEnemies.Count == 0)
            {
                enemyCountText.text = ""; // Clear enemy count text when game is complete
            }
        }
        else
        {
            Debug.LogWarning("Enemy Count Text UI element not assigned!");
        }

        // Check if all enemies are destroyed, no enemies left to spawn, and wave limit not reached
        if (enemiesToSpawn.Count == 0 && activeEnemies.Count == 0 && waveTimer <= 0 && currentWave <= maxWaves)
        {
            currentWave++;
            GenerateWave();
        }

        if (spawnTimer <= 0 && enemiesToSpawn.Count > 0)
        {
            // Select a random spawn location
            Transform selectedSpawn = spawnLocations.Count > 0 ? spawnLocations[Random.Range(0, spawnLocations.Count)] : transform;

            // Find a non-overlapping spawn position
            Vector3 spawnPosition = GetNonOverlappingSpawnPosition(selectedSpawn.position);

            // Calculate initial rotation
            Quaternion spawnRotation = Quaternion.identity;
            if (facePlayerOnSpawn && player != null)
            {
                Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
                if (directionToPlayer != Vector3.zero)
                {
                    float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f; // Adjust for sprite orientation
                    spawnRotation = Quaternion.Euler(0, 0, angle);
                }
            }
            else
            {
                float angle = Mathf.Atan2(customFacingDirection.y, customFacingDirection.x) * Mathf.Rad2Deg - 90f; // Adjust for sprite orientation
                spawnRotation = Quaternion.Euler(0, 0, angle);
            }

            // Spawn an enemy with initial rotation at selected spawn position
            GameObject enemy = Instantiate(enemiesToSpawn[0], spawnPosition, spawnRotation);
            // Track the spawned enemy
            activeEnemies.Add(enemy);
            Debug.Log($"Spawned enemy at {spawnPosition}. Active enemies: {activeEnemies.Count}, To spawn: {enemiesToSpawn.Count}");
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
            spawnTimer -= Time.deltaTime;
            waveTimer -= Time.deltaTime;
        }

        // Clean up destroyed enemies from activeEnemies list
        int removedCount = activeEnemies.RemoveAll(enemy => enemy == null);
        if (removedCount > 0)
        {
            destroyedEnemies += removedCount; // Increment destroyed count
            if (destroyedEnemies > maxEnemiesThisWave)
            {
                destroyedEnemies = maxEnemiesThisWave; // Cap at max enemies
            }
            Debug.Log($"Removed {removedCount} destroyed enemies. Total destroyed: {destroyedEnemies}, Active enemies: {activeEnemies.Count}");
        }
    }

    // NEW: Find a non-overlapping spawn position
    private Vector3 GetNonOverlappingSpawnPosition(Vector3 basePosition)
    {
        Vector3 spawnPosition = basePosition;
        int maxAttempts = 5; // Limit attempts to avoid infinite loops
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            bool isOverlapping = false;
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    float distance = Vector3.Distance(spawnPosition, enemy.transform.position);
                    if (distance < spawnRadius)
                    {
                        isOverlapping = true;
                        break;
                    }
                }
            }

            if (!isOverlapping)
            {
                return spawnPosition; // Position is clear
            }

            // Offset position randomly within maxSpawnOffset
            Vector2 offset = Random.insideUnitCircle * maxSpawnOffset;
            spawnPosition = basePosition + new Vector3(offset.x, offset.y, 0);
            attempt++;
        }

        // If no clear position found, return original position with warning
        Debug.LogWarning($"Could not find non-overlapping spawn position after {maxAttempts} attempts at {basePosition}. Using original position.");
        return basePosition;
    }

    public void GenerateWave()
    {
        // Only generate wave if within maxWaves limit
        if (currentWave <= maxWaves)
        {
            waveValue = currentWave * 10; // Set waveValue based on currentWave
            destroyedEnemies = 0; // Reset destroyed count for new wave
            GenerateEnemies();

            // Set max enemies for this wave
            maxEnemiesThisWave = enemiesToSpawn.Count; // Store initial enemy count for the wave
            Debug.Log($"Generated wave {currentWave} with {maxEnemiesThisWave} enemies to spawn");

            // Update wave text and start fade
            if (waveText != null)
            {
                waveText.text = $"Wave {currentWave}";
                if (waveTextCanvasGroup != null)
                {
                    waveTextCanvasGroup.alpha = 1f; // Reset to fully visible
                    StartCoroutine(FadeWaveText()); // Start fade animation
                }
                else
                {
                    Debug.LogWarning("Wave Text CanvasGroup not assigned! Fade animation will not work.");
                }
            }

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
    }

    private IEnumerator FadeWaveText()
    {
        if (waveTextCanvasGroup == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            waveTextCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null; // Wait for the next frame
        }
        waveTextCanvasGroup.alpha = 0f; // Ensure fully transparent
        Debug.Log($"Wave text faded out for Wave {currentWave}");
    }

    public void GenerateEnemies()
    {
        List<GameObject> generatedEnemies = new List<GameObject>();
        if (enemies.Count == 0)
        {
            Debug.LogWarning("No enemies assigned to WaveSpawner!");
            return;
        }

        // Get the max enemies for this wave (use last value if wave exceeds list length)
        int waveMaxEnemies = maxEnemiesPerWave[Mathf.Min(currentWave - 1, maxEnemiesPerWave.Count - 1)];

        int enemyCount = 0; // Track number of enemies generated
        while (waveValue > 0 && enemyCount < waveMaxEnemies)
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
        Debug.Log($"Generated {generatedEnemies.Count} enemies for wave {currentWave}");
    }
}

[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int cost;
}