using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public int waveNumber;
    public int basicCount;
    public int spitterCount;
    public int tankCount;
    public bool bossWave;
    public float spawnInterval = 2f;
}

public class LevelWaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public Wave[] waves;
    private int currentWaveIndex = 0;

    [Header("Monster Prefabs")]
    public GameObject walkerPrefab;
    public GameObject spitterPrefab;
    public GameObject tankPrefab;
    public GameObject bossPrefab;

    [Header("Spawn Settings")]
    public Transform pipe;               // The player ring/center
    public float spawnDistance = 20f;    // How far from the ring to spawn
    public float spawnHeight = 5f;     // Height above ground

    [Header("Wave Timing")]
    public float timeBetweenWaves = 5f;
    private bool isSpawning = false;
    private bool allWavesComplete = false;

    [Header("Wave Sounds")]
    public AudioClip waveStartSound;
    public AudioClip waveCompleteSound;
    private AudioSource audioSource;

    void Start()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound for UI/announcements
        }

        // Find the Pipe if not assigned
        if (pipe == null)
        {
            pipe = GameObject.Find("Pipe").transform;
            if (pipe == null)
            {
                Debug.LogError("Pipe not found! Assign it in the inspector.");
            }
        }

        Debug.Log("LevelWaveManager started. Starting first wave in 3 seconds...");
        Invoke("StartFirstWave", 3f);
    }

    void StartFirstWave()
    {
        if (waves.Length > 0)
        {
            StartCoroutine(SpawnWave(currentWaveIndex));
        }
        else
        {
            Debug.LogWarning("No waves configured!");
        }
    }

    IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Length) yield break;

        Wave wave = waves[waveIndex];
        Debug.Log($"=== STARTING WAVE {waveIndex + 1} ===");
        Debug.Log($"Walkers: {wave.basicCount}, Spitters: {wave.spitterCount}, Tanks: {wave.tankCount}");

        // Play wave start sound
        PlayWaveStartSound();

        isSpawning = true;

        // Spawn walkers (basic monsters)
        for (int i = 0; i < wave.basicCount; i++)
        {
            SpawnMonster(walkerPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        // Spawn spitters
        for (int i = 0; i < wave.spitterCount; i++)
        {
            SpawnMonster(spitterPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        // Spawn tanks
        for (int i = 0; i < wave.tankCount; i++)
        {
            SpawnMonster(tankPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        // Spawn boss
        if (wave.bossWave)
        {
            SpawnMonster(bossPrefab);
        }

        isSpawning = false;

        // Play wave complete sound
        PlayWaveCompleteSound();

        currentWaveIndex++;

        // Check if there are more waves
        if (currentWaveIndex < waves.Length)
        {
            Debug.Log($"Wave {waveIndex + 1} complete! Next wave in {timeBetweenWaves} seconds...");
            yield return new WaitForSeconds(timeBetweenWaves);
            StartCoroutine(SpawnWave(currentWaveIndex));
        }
        else
        {
            Debug.Log("ALL WAVES COMPLETE!");
            allWavesComplete = true;
            // Trigger boss battle or game win here
        }
    }

    void PlayWaveStartSound()
    {
        if (waveStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(waveStartSound);
            Debug.Log($"Playing wave start sound for wave {currentWaveIndex + 1}");
        }
    }

    void PlayWaveCompleteSound()
    {
        if (waveCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(waveCompleteSound);
            Debug.Log($"Playing wave complete sound for wave {currentWaveIndex + 1}");
        }
    }

    void SpawnMonster(GameObject monsterPrefab)
    {
        if (monsterPrefab == null || pipe == null)
        {
            Debug.LogError("Missing prefab or pipe reference!");
            return;
        }

        // Get random angle around the pipe (0 to 360 degrees)
        float randomAngle = Random.Range(0f, 360f);

        // Calculate spawn position using trigonometry
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * spawnDistance;
        float z = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * spawnDistance;

        Vector3 spawnPosition = pipe.position + new Vector3(x, 0f, z);

        // Create the monster
        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);

        // Make the monster face the pipe/center
        monster.transform.LookAt(pipe.position);

        // Optional: Adjust rotation to keep monster upright
        monster.transform.eulerAngles = new Vector3(0, monster.transform.eulerAngles.y, 0);

        Debug.Log($"Spawned {monsterPrefab.name} at position: {spawnPosition}");
    }

    void OnDrawGizmos()
    {
        if (pipe == null) return;

        // Draw spawn circle in Scene view
        Gizmos.color = Color.red;
        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f * Mathf.Deg2Rad;
            Vector3 point1 = pipe.position + new Vector3(
                Mathf.Cos(angle) * spawnDistance,
                spawnHeight,
                Mathf.Sin(angle) * spawnDistance
            );

            float nextAngle = (i + 1) * 10f * Mathf.Deg2Rad;
            Vector3 point2 = pipe.position + new Vector3(
                Mathf.Cos(nextAngle) * spawnDistance,
                spawnHeight,
                Mathf.Sin(nextAngle) * spawnDistance
            );

            Gizmos.DrawLine(point1, point2);
        }

        // Draw defense ring (where monsters breach)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pipe.position, 5f); // Adjust radius to match your Pipe size
    }

    public bool AreAllWavesComplete()
    {
        return allWavesComplete;
    }

    public void bossSummon(int walkerNum, int tankNum, int spitterNum)
    {
        for (int i = 0; i < walkerNum; i++)
        {
            SpawnMonster(walkerPrefab);
        }
        for (int i = 0; i < tankNum; i++)
        {
            SpawnMonster(tankPrefab);
        }
        for (int i = 0; i < spitterNum; i++)
        {
            SpawnMonster(spitterPrefab);
        }
    }
}