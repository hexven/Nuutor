using UnityEngine;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    public List<Enemy> enemies = new List<Enemy>(); // Initialize the list
    public int currentWave;
    public int waveValue;
    public List<GameObject> enemiesToSpawn = new List<GameObject>(); // Corrected capitalization

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateWave();
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    public void GenerateWave()
    {
        waveValue = currentWave * 10; // Fixed: Set waveValue based on currentWave
        GenerateEnemies(); // Fixed: Correct method name
    }

    public void GenerateEnemies() // Fixed: Correct method name
    {
        List<GameObject> generatedEnemies = new List<GameObject>(); // Changed to List<GameObject>
        while (waveValue > 0)
        {
            int randEnemy = Random.Range(0, enemies.Count);
            int randEnemyCost = enemies[randEnemy].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemy].enemyPrefab); // Add enemyPrefab (GameObject)
                waveValue -= randEnemyCost;
            }
            else
            {
                break; // Exit loop if waveValue is too low
            }
        }
        enemiesToSpawn.Clear(); // Fixed: Correct capitalization and method
        enemiesToSpawn = generatedEnemies; // Assign the generated list
    }
}

[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int cost;
}