using UnityEngine;

public class HazardVolume : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to any entity entering this zone. Set high for instant kill.")]
    public float damageAmount = 9999f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the volume has an EntityHealth component
        EntityHealth health = other.GetComponentInParent<EntityHealth>();

        if (health != null)
        {
            // Deal massive damage to trigger the Die() function immediately
            health.TakeDamage(damageAmount);
            Debug.Log($"[HAZARD] {other.gameObject.name} fell into {gameObject.name} and was eliminated.");
        }
    }
}