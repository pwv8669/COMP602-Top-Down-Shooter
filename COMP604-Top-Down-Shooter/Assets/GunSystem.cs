using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

/// <summary>
/// Gun shooting system with multiplayer support
/// Uses raycast for hit detection and syncs across network
/// FIXED: Properly finds PhotonView on parent Character
/// </summary>
public class GunSystem : MonoBehaviourPunCallbacks
{
    [Header("Selected weapon stats (set at match start)")]
    public int damage = 10;
    public float fireRate = 8f;          // shots per second
    public float spreadDegrees = 1.5f;   // random cone around aim ray
    public float range = 100f;
    public float reloadTime = 1.0f;
    public int magazineSize = 12;
    public int bulletsPerTap = 1;        // 1 = single; >1 = burst per click (semi)
    public bool isAutomatic = false;     // true = hold to fire

    [Header("Scene refs")]
    public Camera mainCamera;            // top-down camera
    public LayerMask targetMask;         // enemies

    [Header("Audio")]
    public AudioSource audioSource;      // assign in inspector
    public AudioClip audioClip;          // assign in inspector

    // Runtime
    int bulletsLeft;
    int shotsRemainingThisTap;
    bool readyToShoot = true;
    bool reloading = false;
    bool fireHeld = false;
    float nextShotTime = 0f;

    // FIXED: Cache parent PhotonView
    private PhotonView parentPhotonView;

    void Awake()
    {
        bulletsLeft = magazineSize;
        if (!mainCamera) mainCamera = Camera.main;

        // FIXED: Find PhotonView on parent Character, not on Gun
        parentPhotonView = GetComponentInParent<PhotonView>();

        if (parentPhotonView == null)
        {
            Debug.LogWarning("[GunSystem] No PhotonView found on parent! Multiplayer features disabled.");
        }
    }

    void Update()
    {
        // MULTIPLAYER: Only local player can shoot
        if (parentPhotonView != null && !parentPhotonView.IsMine)
            return;

        // Direct input handling
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartCoroutine(ReloadRoutine());
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !isAutomatic)
        {
            BeginTapFire();
        }

        if (Mouse.current.leftButton.isPressed && isAutomatic)
        {
            TryShootOnce();
        }

        // Keep trying to shoot while held
        if (isAutomatic && fireHeld)
            TryShootOnce();
    }

    // ===== New Input System callbacks =====
    public void OnFire(InputAction.CallbackContext context)
    {
        // MULTIPLAYER: Only local player
        if (parentPhotonView != null && !parentPhotonView.IsMine)
            return;

        if (context.performed)
        {
            fireHeld = true;
            if (!isAutomatic) BeginTapFire(); // semi/burst: fire on press
        }
        else if (context.canceled)
        {
            fireHeld = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        // MULTIPLAYER: Only local player
        if (parentPhotonView != null && !parentPhotonView.IsMine)
            return;

        if (!context.performed) return;
        if (reloading) return;
        if (bulletsLeft >= magazineSize) return; // already full
        StartCoroutine(ReloadRoutine());
    }

    // ===== Core shooting =====
    void BeginTapFire()
    {
        shotsRemainingThisTap = bulletsPerTap;
        TryShootOnce();
    }

    void TryShootOnce()
    {
        // Basic checks
        if (reloading) return;
        if (!readyToShoot) return;
        if (Time.time < nextShotTime) return;
        if (bulletsLeft <= 0) return;
        if (!mainCamera) return;

        readyToShoot = false;

        // Step 1: Raycast from camera through mouse
        Ray camRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(camRay, out RaycastHit camHit, Mathf.Infinity))
        {
            // Step 2: Set target position for bullet raycast
            Vector3 targetPos = new Vector3(camHit.point.x, transform.position.y, camHit.point.z);

            // Step 3: Calculate direction from gun to target point
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0; // Keep it horizontal
            direction = direction.normalized;

            // Step 4: Apply spread
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                Quaternion spreadRot = Quaternion.AngleAxis(yaw, Vector3.up);
                direction = spreadRot * direction;
            }

            // Step 5: Raycast from gun towards direction
            Vector3 shootOrigin = new Vector3(transform.position.x, 1.0f, transform.position.z); // Character height

            if (Physics.Raycast(shootOrigin, direction, out RaycastHit enemyHit, range, targetMask))
            {
                Health enemyHealth = enemyHit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    // MULTIPLAYER: Send damage (Health.cs handles routing)
                    enemyHealth.TakeDamage(damage, enemyHit.point);
                }
            }

            // Play shoot effects locally (each player hears their own gun)
            PlayShootEffects();

            // Ammo management
            bulletsLeft--;
            float secondsPerShot = 1f / Mathf.Max(0.0001f, fireRate);
            nextShotTime = Time.time + secondsPerShot;
            Invoke(nameof(ResetShot), secondsPerShot * 0.9f);

            // Burst continuation
            shotsRemainingThisTap = Mathf.Max(0, shotsRemainingThisTap - 1);
            if (!isAutomatic && shotsRemainingThisTap > 0 && bulletsLeft > 0)
                Invoke(nameof(TryShootOnce), secondsPerShot);
        }
        else
        {
            readyToShoot = true;
        }
    }

    void PlayShootEffects()
    {
        if (audioSource && audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }
        // Add muzzle flash, particles, etc. here
    }

    void ResetShot() => readyToShoot = true;

    System.Collections.IEnumerator ReloadRoutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = magazineSize;
        reloading = false;
    }

    // Public helper if you set stats from a ScriptableObject at match start
    public void ApplyStats(
        int dmg, float rof, float spreadDeg, float rng, float reloadSec,
        int magSize, int perTap, bool automatic, bool refillMag = true)
    {
        damage = dmg;
        fireRate = rof;
        spreadDegrees = spreadDeg;
        range = rng;
        reloadTime = reloadSec;
        magazineSize = magSize;
        bulletsPerTap = Mathf.Max(1, perTap);
        isAutomatic = automatic;

        if (refillMag)
            bulletsLeft = magazineSize;

        // Reset gates
        reloading = false;
        readyToShoot = true;
        nextShotTime = 0f;
        fireHeld = false;
    }
}