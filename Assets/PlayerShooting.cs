using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public WeaponManager weaponManager;
    private float nextTimeToFire = 0f;

    void Update()
    {
        if (weaponManager == null) return;

        WeaponData activeWeapon = weaponManager.GetCurrentWeapon();
        if (activeWeapon == null) return;

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + activeWeapon.fireRate;
            Shoot(activeWeapon);
        }
    }

    void Shoot(WeaponData weapon)
    {
        if (weapon.muzzleFlash != null)
        {
            weapon.muzzleFlash.Play();
        }

        // Fire straight through the middle viewport point of the active camera frame
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, weapon.range))
        {
            Debug.Log($"Hit {hit.transform.name}!");

            // Look for the clean unified EntityHealth script
            EntityHealth targetHealth = hit.transform.GetComponent<EntityHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weapon.damage);
            }
        }
    }
}
