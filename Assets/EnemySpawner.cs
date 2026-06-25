using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public int totalEnemies = 30;

    [Header("World Settings")]
    public MeshFilter targetMeshFilter;
    public float waterLevel = 4f;

    void Start()
    {
        // 2-second fallback remains active
        Invoke("StartSpawning", 2f);
    }

    public void StartSpawning()
    {
        // Cancel the invoke if this was called manually by the map generator
        CancelInvoke("StartSpawning");

        if (targetMeshFilter == null)
        {
            Debug.LogError("Enemy Spawner: targetMeshFilter is missing in the Inspector!");
            return;
        }

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        int spawnedCount = 0;
        int safetyNet = 0;

        Bounds meshBounds = targetMeshFilter.GetComponent<Renderer>().bounds;

        // Diagnostic helper logs
        Debug.Log($"Spawner Bounds Info -> Min: {meshBounds.min}, Max: {meshBounds.max}, Center: {meshBounds.center}");

        while (spawnedCount < totalEnemies && safetyNet < 1500)
        {
            safetyNet++;

            // Pick a random coordinate within the mesh bounds
            float randomX = Random.Range(meshBounds.min.x, meshBounds.max.x);
            float randomZ = Random.Range(meshBounds.min.z, meshBounds.max.z);

            // Cast from extremely high up down into the center of the world
            float raycastStartHeight = meshBounds.max.y + 100f;
            Vector3 origin = new Vector3(randomX, raycastStartHeight, randomZ);

            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit hit;

            // Shoot downwards through everything
            if (Physics.Raycast(ray, out hit, 2000f))
            {
                // REDUCED RESTRICTIONS: If it hits anything with a collider above water level
                if (hit.point.y > waterLevel)
                {
                    Vector3 spawnPos = hit.point + new Vector3(0f, 1.5f, 0f);

                    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    enemy.name = $"CombatEnemy_{spawnedCount + 1}";

                    InitializeEnemy(enemy);
                    spawnedCount++;
                }
            }
        }

        Debug.Log($"FINAL REPORT: Successfully spawned {spawnedCount} enemies on dry land! Total attempts: {safetyNet}");
    }

    void InitializeEnemy(GameObject enemy)
    {
        WeaponManager enemyWeaponManager = enemy.GetComponentInChildren<WeaponManager>();
        if (enemyWeaponManager != null)
        {
            enemyWeaponManager.enabled = true;
        }
    }
}
