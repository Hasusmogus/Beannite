using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName = "Pistol";
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.2f; // Cooldown time between shots in seconds

    [Header("Effects")]
    public ParticleSystem muzzleFlash; // Optional: Drag a flash particle here later
}
