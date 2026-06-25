using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("AI Settings")]
    public bool isAI = false;

    [Header("Weapon Inventory Data")]
    public WeaponData[] availableWeapons;
    private int currentWeaponIndex = 0;

    void Start()
    {
        EquipWeapon(0);
    }

    void Update()
    {
        // Only handle manual switching for the human player
        if (!isAI)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);

            if (Input.GetAxis("Mouse ScrollWheel") > 0f) EquipWeapon(currentWeaponIndex + 1);
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) EquipWeapon(currentWeaponIndex - 1);
        }
    }

    public void EquipWeapon(int index)
    {
        if (transform.childCount == 0) return;

        if (index < 0) index = transform.childCount - 1;
        if (index >= transform.childCount) index = 0;

        currentWeaponIndex = index;

        int i = 0;
        foreach (Transform weaponModel in transform)
        {
            weaponModel.gameObject.SetActive(i == index);
            i++;
        }
    }

    public WeaponData GetCurrentWeapon()
    {
        if (availableWeapons == null || availableWeapons.Length == 0) return null;
        if (currentWeaponIndex < availableWeapons.Length) return availableWeapons[currentWeaponIndex];
        return null;
    }

    // --- NEW CRUCIAL HELPER ---
    // Finds the live particle system on the currently active gun model in the world
    public ParticleSystem GetActiveMuzzleFlash()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < transform.childCount)
        {
            Transform activeWeaponModel = transform.GetChild(currentWeaponIndex);
            return activeWeaponModel.GetComponentInChildren<ParticleSystem>();
        }
        return null;
    }
}
