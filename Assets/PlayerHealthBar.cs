using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private Slider healthSlider;
    private EntityHealth playerHealth;

    void Start()
    {
        healthSlider = GetComponent<Slider>();

        // Find the player entity in the scene
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsInactive.Exclude);
        foreach (EntityHealth entity in allEntities)
        {
            if (entity.isPlayer)
            {
                playerHealth = entity;
                break;
            }
        }

        // Initialize slider values
        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }
        else
        {
            Debug.LogError("[Health Bar] Could not find an EntityHealth script marked as 'isPlayer'!");
        }
    }

    void Update()
    {
        if (playerHealth != null && healthSlider != null)
        {
            // Update the slider to match the player's current health frame-by-frame
            healthSlider.value = playerHealth.GetCurrentHealth();
        }
    }
}