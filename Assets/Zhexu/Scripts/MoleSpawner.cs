using UnityEngine;

public class MoleSpawner : MonoBehaviour
{
    [Header("Prefabs (Mole Prefabs)")]
    public GameObject moleNormalPrefab;    // Normal mole
    public GameObject moleExplosivePrefab; // Explosive mole

    [Header("Settings (Spawn Settings)")]
    [Range(0f, 1f)] public float explosiveChance = 0.3f; // 30% chance to spawn explosive mole
    public float minSpawnTime = 2.0f;      // After a mole dies, minimum 2 seconds before next spawn
    public float maxSpawnTime = 5.0f;      // Maximum 5 seconds before next spawn

    [Header("Spawn Position")]
    public Transform spawnPoint;           // Specific spawn point

    private GameObject currentMole;        // Reference to the current mole in the hole
    private float cooldownTimer;           // Countdown timer

    void Start()
    {
        // At game start, give a random cooldown to avoid all holes spawning simultaneously
        ResetTimer();
    }

    void Update()
    {
        // 1. Check if a mole already exists
        // If currentMole is not null, the mole is alive — do nothing and wait
        if (currentMole != null) return;

        // 2. If currentMole is null (mole was hit, exploded, or disappeared)
        // Start counting down
        cooldownTimer -= Time.deltaTime;

        // 3. When countdown ends, spawn a new mole
        if (cooldownTimer <= 0f)
        {
            SpawnMole();
            ResetTimer(); // Reset timer for the next mole
        }
    }

    void SpawnMole()
    {
        // Randomly choose which type of mole to spawn
        GameObject prefabToUse = (Random.value < explosiveChance) ? moleExplosivePrefab : moleNormalPrefab;

        if (prefabToUse != null && spawnPoint != null)
        {
            // Instantiate the mole and store its reference in currentMole
            currentMole = Instantiate(prefabToUse, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"SpawnMole called for {name}", this);
        }
    }

    void ResetTimer()
    {
        cooldownTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    public void bossSummonExplosive()
    {
        Destroy(currentMole);
        currentMole = Instantiate(moleExplosivePrefab, spawnPoint.position, spawnPoint.rotation);
        ResetTimer();
    }
}
