using UnityEngine;
// CRITICAL: Namespace needed for building pathfinding data at runtime
using Unity.AI.Navigation; 

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class IslandGenerator : MonoBehaviour
{
    [Header("Spawning References")]
    public EnemySpawner enemySpawner;
    public GameObject playerObject;

    [Header("Navigation Setup")]
    [Tooltip("Drag your NavMeshSurface GameObject here from the scene hierarchy.")]
    public NavMeshSurface navMeshSurface;

    [Header("Map Dimensions")]
    public int mapSize = 300;
    public float noiseScale = 15f;
    public float heightMultiplier = 200f;
    public AnimationCurve heightCurve;

    [Header("Biome Heights (0.0 to 1.0)")]
    public float sandHeight = 0.15f;
    public float grassHeight = 0.55f;
    public float rockHeight = 0.8f;

    [Header("Biome Colors")]
    public Color sandColor = new Color(0.92f, 0.83f, 0.61f);
    public Color grassColor = new Color(0.34f, 0.65f, 0.32f);
    public Color rockColor = new Color(0.5f, 0.45f, 0.41f);
    public Color snowColor = new Color(0.95f, 0.95f, 0.95f);

    [Header("Seed info")]
    public float seed;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (heightCurve == null || heightCurve.keys.Length == 0)
        {
            heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        GenerateIsland();
    }

    void GenerateIsland()
    {
        seed = Random.Range(0f, 100000f);

        float[,] heightMap = new float[mapSize + 1, mapSize + 1];

        for (int y = 0; y <= mapSize; y++)
        {
            for (int x = 0; x <= mapSize; x++)
            {
                float sampleX = (x + seed) / noiseScale;
                float sampleY = (y + seed) / noiseScale;
                float noise = Mathf.PerlinNoise(sampleX, sampleY);

                float a = (x / (float)mapSize) * 2f - 1f;
                float b = (y / (float)mapSize) * 2f - 1f;

                float value = Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));

                float p = 3f;
                float falloff = Mathf.Pow(value, p) / (Mathf.Pow(value, p) + Mathf.Pow(2.2f - 2.2f * value, p));

                float finalHeight = noise - falloff;
                finalHeight = Mathf.Clamp01(finalHeight);

                heightMap[x, y] = heightCurve.Evaluate(finalHeight) * heightMultiplier;
            }
        }

        CreateLowPolyMesh(heightMap);
    }

    void CreateLowPolyMesh(float[,] heightMap)
    {
        int numQuads = mapSize * mapSize;
        int numTriangles = numQuads * 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];
        Color[] colors = new Color[numVertices];

        int vIdx = 0;
        float halfSize = mapSize / 2f;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float hBL = heightMap[x, y];
                float hBR = heightMap[x + 1, y];
                float hTL = heightMap[x, y + 1];
                float hTR = heightMap[x + 1, y + 1];

                Vector3 bl = new Vector3(x - halfSize, hBL, y - halfSize);
                Vector3 br = new Vector3(x + 1 - halfSize, hBR, y - halfSize);
                Vector3 tl = new Vector3(x - halfSize, hTL, y + 1 - halfSize);
                Vector3 tr = new Vector3(x + 1 - halfSize, hTR, y + 1 - halfSize);

                Color tri1Color = GetColorFromHeight((hBL + hTL + hBR) / 3f);
                Color tri2Color = GetColorFromHeight((hTL + hTR + hBR) / 3f);

                vertices[vIdx] = bl; colors[vIdx] = tri1Color; triangles[vIdx] = vIdx; vIdx++;
                vertices[vIdx] = tl; colors[vIdx] = tri1Color; triangles[vIdx] = vIdx; vIdx++;
                vertices[vIdx] = br; colors[vIdx] = tri1Color; triangles[vIdx] = vIdx; vIdx++;

                vertices[vIdx] = tl; colors[vIdx] = tri2Color; triangles[vIdx] = vIdx; vIdx++;
                vertices[vIdx] = tr; colors[vIdx] = tri2Color; triangles[vIdx] = vIdx; vIdx++;
                vertices[vIdx] = br; colors[vIdx] = tri2Color; triangles[vIdx] = vIdx; vIdx++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        if (playerObject != null)
        {
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 playerRayOrigin = new Vector3(0f, heightMultiplier + 50f, 0f);
            Ray playerRay = new Ray(playerRayOrigin, Vector3.down);
            RaycastHit playerHit;

            if (Physics.Raycast(playerRay, out playerHit, heightMultiplier + 200f))
            {
                playerObject.transform.position = playerHit.point + new Vector3(0f, 2f, 0f);
            }
            else
            {
                playerObject.transform.position = new Vector3(0f, heightMultiplier / 2f, 0f);
            }

            playerObject.SetActive(true);

            if (cc != null) cc.enabled = true;
            Debug.Log("Island Generator: Player safely snapped to surface layer cleanly!");
        }

        // --- RUNTIME NAVMESH BAKE TRIGGER ---
        // Forces the pathfinding network to generate AFTER player positioning but BEFORE spawning AI enemies
        BuildNavigationAtRuntime();

        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
        }
    }

    void BuildNavigationAtRuntime()
    {
        if (navMeshSurface != null)
        {
            Debug.Log("[GENERATOR] Low-poly mesh completed! Baking runtime AI path grid now...");
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("[GENERATOR] NavMeshSurface link missing on inspector panel. Enemy path loops might run blindly into hazardous boundaries!");
        }
    }

    Color GetColorFromHeight(float height)
    {
        float normalizedHeight = height / heightMultiplier;

        if (normalizedHeight < sandHeight) return sandColor;
        if (normalizedHeight < grassHeight) return grassColor;
        if (normalizedHeight < rockHeight) return rockColor;
        return snowColor;
    }
}