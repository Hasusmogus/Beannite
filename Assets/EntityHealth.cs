using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [HideInInspector] public bool isDead = false;

    [Header("Grace Period (Seconds)")]
    public float gracePeriodDuration = 2.0f;
    private float spawnTime;

    [Header("Components to Disable on Death")]
    public MonoBehaviour movementScript; 

    [Header("Entity Type")]
    public bool isPlayer = false;

    [Header("Spawner Fix")]
    [Tooltip("Check this TRUE only on the base enemy template in the scene that your spawner relies on!")]
    public bool isTemplateForGeneration = false;

    private static EntityHealth masterSceneTemplateInstance = null;
    private bool isActualSceneTemplate = false;

    void Awake()
    {
        if (isTemplateForGeneration && masterSceneTemplateInstance == null)
        {
            masterSceneTemplateInstance = this;
            isActualSceneTemplate = true;
        }

        if (!System.Object.ReferenceEquals(masterSceneTemplateInstance, this))
        {
            isTemplateForGeneration = false;
            isActualSceneTemplate = false;
        }

        if (isActualSceneTemplate)
        {
            // Teleport the master template deep into the void out of sight
            transform.position = new Vector3(0, -10000f, 0);

            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            
            if (movementScript != null) movementScript.enabled = false;

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders) if (col != null) col.enabled = false;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers) if (rend != null) rend.enabled = false;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        spawnTime = Time.time;

        if (!System.Object.ReferenceEquals(masterSceneTemplateInstance, this))
        {
            isTemplateForGeneration = false;
            isActualSceneTemplate = false;

            gameObject.layer = LayerMask.NameToLayer("Enemy"); 
            
            if (movementScript != null) movementScript.enabled = true;

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders) if (col != null) col.enabled = true;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers) if (rend != null) rend.enabled = true;
        }
    }

    // --- NEW PUBLIC GETTER: Allows the UI Health Bar Slider to read this value ---
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isActualSceneTemplate) return;

        bool isRealPlayer = isPlayer;
        if (movementScript is PlayerController pc && pc.isAI)
        {
            isRealPlayer = false; 
        }

        if (isRealPlayer && (Time.time - spawnTime) < gracePeriodDuration)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isActualSceneTemplate) return; 

        isDead = true;

        if (movementScript != null)
            movementScript.enabled = false;

        bool isRealPlayer = isPlayer;
        if (movementScript is PlayerController pc && pc.isAI)
        {
            isRealPlayer = false; 
        }

        if (isRealPlayer)
        {
            Debug.Log("The real Human Player has died!");

            DeathScreenManager deathScreen = FindAnyObjectByType<DeathScreenManager>(FindObjectsInactive.Include);
            if (deathScreen != null)
            {
                deathScreen.ShowDeathScreen();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} (FFA Clone/Enemy) has died!");
            
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders) if (col != null) col.enabled = false;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers) if (rend != null) rend.enabled = false;

            Destroy(gameObject, 5.0f); 
        }
    }
}