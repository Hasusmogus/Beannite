using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Entity Controls")]
    public bool isAI = false;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 3f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look Settings")]
    public float mouseSensitivity = 200f;
    public Transform cameraTransform;

    [Header("Combat Settings")]
    public float detectionRadius = 40f;
    private Transform combatTarget;
    private float nextShootTime = 0f;
    private WeaponManager weaponManager;

    [Header("Audio Customization")]
    [Tooltip("Slightly variation in pitch for every shot so the sound feels organic.")]
    [Range(0f, 0.3f)] public float pitchRandomness = 0.05f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    private Vector3 aiMoveDirection;
    private float aiStateTimer = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        weaponManager = GetComponentInChildren<WeaponManager>();

        if (characterController == null)
        {
            enabled = false;
            return;
        }

        if (!isAI)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            GetNewAIWanderDirection();
        }

        // Ignore self-collisions across all child components
        Collider[] myColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in myColliders)
        {
            if (col != characterController && col != null)
            {
                Physics.IgnoreCollision(characterController, col, true);
            }
        }
    }

    void Update()
    {
        if (characterController == null || !characterController.enabled) return;

        HandleGravity();

        Vector3 structuralMoveDirection = Vector3.zero;

        if (!isAI)
        {
            structuralMoveDirection = GetHumanInputDirection();
            HandleHumanLook();
            HandleHumanShooting();
        }
        else
        {
            structuralMoveDirection = GetAIInputDirection();
        }

        Vector3 finalMovementFrame = structuralMoveDirection * moveSpeed;
        finalMovementFrame.y = velocity.y;

        characterController.Move(finalMovementFrame * Time.deltaTime);
    }

    private void HandleGravity()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
    }

    private Vector3 GetHumanInputDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveInput = transform.right * x + transform.forward * z;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput *= sprintMultiplier;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        return moveInput;
    }

    private void HandleHumanLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleHumanShooting()
    {
        if (weaponManager == null || Time.time < nextShootTime) return;

        WeaponData activeGun = weaponManager.GetCurrentWeapon();
        if (activeGun == null) return;

        if (Input.GetButton("Fire1"))
        {
            nextShootTime = Time.time + activeGun.fireRate;

            // --- AUDIO SYSTEM INTEGRATION ---
            PlayActiveWeaponSound();

            // --- FIXED MUZZLE FLASH SYSTEM OVERRIDE ---
            ParticleSystem flash = weaponManager.GetActiveMuzzleFlash();
            if (flash != null)
            {
                flash.gameObject.SetActive(true);

                // Construct parameters to bypass inspector modules set to 0 particles
                ParticleSystem.EmitParams forceBurst = new ParticleSystem.EmitParams();
                flash.Emit(forceBurst, 2);
            }

            Vector3 fireOrigin = cameraTransform != null ? cameraTransform.position : transform.position + new Vector3(0, 1.5f, 0);
            Vector3 fireDirection = cameraTransform != null ? cameraTransform.forward : transform.forward;

            Ray ray = new Ray(fireOrigin, fireDirection);
            RaycastHit hit;

            int targetMask = LayerMask.GetMask("Enemy");

            if (Physics.Raycast(ray, out hit, activeGun.range, targetMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == null) return;

                EntityHealth hitHealth = hit.transform.GetComponentInParent<EntityHealth>();
                if (hitHealth != null)
                {
                    hitHealth.TakeDamage(activeGun.damage);
                }
            }
        }
    }

    private Vector3 GetAIInputDirection()
    {
        FindClosestTarget();

        if (combatTarget != null && combatTarget.gameObject != null && combatTarget.gameObject.activeInHierarchy)
        {
            Vector3 lookDirection = (combatTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 8f);
            }

            float distanceToTarget = Vector3.Distance(transform.position, combatTarget.position);
            if (distanceToTarget > 15f)
            {
                aiMoveDirection = lookDirection;
            }
            else
            {
                aiMoveDirection = Vector3.zero;
            }

            HandleAIShooting();
        }
        else
        {
            combatTarget = null;

            if (aiMoveDirection == Vector3.zero || aiStateTimer <= 0f)
            {
                GetNewAIWanderDirection();
            }

            aiStateTimer -= Time.deltaTime;

            if (aiMoveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aiMoveDirection), Time.deltaTime * 5f);
            }
        }

        return aiMoveDirection;
    }

    private void FindClosestTarget()
    {
        int targetMask = LayerMask.GetMask("Player", "Enemy");
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, detectionRadius, targetMask);

        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider potentialTarget in targetsInRadius)
        {
            if (potentialTarget == null || potentialTarget.gameObject == null || !potentialTarget.gameObject.activeInHierarchy) continue;
            if (potentialTarget.transform.root == transform.root) continue;

            EntityHealth targetHealth = potentialTarget.GetComponentInParent<EntityHealth>();
            if (targetHealth == null || targetHealth.isDead) continue;

            float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.transform.position);
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestTarget = potentialTarget.transform;
            }
        }

        combatTarget = bestTarget;
    }

    private void HandleAIShooting()
    {
        if (weaponManager == null || combatTarget == null || Time.time < nextShootTime) return;

        WeaponData activeGun = weaponManager.GetCurrentWeapon();
        if (activeGun == null) return;

        nextShootTime = Time.time + activeGun.fireRate;

        // --- AUDIO SYSTEM INTEGRATION ---
        PlayActiveWeaponSound();

        ParticleSystem flash = weaponManager.GetActiveMuzzleFlash();
        if (flash != null)
        {
            flash.gameObject.SetActive(true);

            ParticleSystem.EmitParams forceBurst = new ParticleSystem.EmitParams();
            flash.Emit(forceBurst, 2);
        }

        Vector3 targetCenter = combatTarget.position;
        Collider targetCollider = combatTarget.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCenter = targetCollider.bounds.center;
        }

        Vector3 fireOrigin = transform.position;
        if (characterController != null)
        {
            fireOrigin = characterController.bounds.center;
        }
        else
        {
            fireOrigin += new Vector3(0, 1f, 0);
        }

        Vector3 fireDirection = (targetCenter - fireOrigin).normalized;

        Ray ray = new Ray(fireOrigin, fireDirection);
        RaycastHit hit;

        int targetMask = LayerMask.GetMask("Player", "Enemy");

        Debug.DrawRay(fireOrigin, fireDirection * activeGun.range, Color.red, 0.2f);

        if (Physics.Raycast(ray, out hit, activeGun.range, targetMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == null || hit.transform.root == transform.root) return;

            EntityHealth hitHealth = hit.transform.GetComponentInParent<EntityHealth>();
            if (hitHealth != null && !hitHealth.isDead)
            {
                hitHealth.TakeDamage(activeGun.damage);
                Debug.Log($"[HIT] {gameObject.name} shot {hit.transform.name}!");
            }
        }
    }

    private void PlayActiveWeaponSound()
    {
        if (weaponManager == null) return;

        // Automatically searches the active weapon model hierarchy for an AudioSource component
        AudioSource weaponAudio = weaponManager.GetComponentInChildren<AudioSource>();
        
        if (weaponAudio != null && weaponAudio.clip != null)
        {
            // Apply slight organic pitch shifting
            weaponAudio.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
            
            // PlayOneShot handles fast overlapping fires smoothly
            weaponAudio.PlayOneShot(weaponAudio.clip);
        }
    }

    private void GetNewAIWanderDirection()
    {
        aiStateTimer = Random.Range(2f, 5f);
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        aiMoveDirection = new Vector3(randomX, 0f, randomZ).normalized;
    }
}