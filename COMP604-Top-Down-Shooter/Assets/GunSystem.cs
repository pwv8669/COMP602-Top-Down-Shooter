using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
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

    // runtime
    int bulletsLeft;
    int shotsRemainingThisTap;
    bool readyToShoot = true;
    bool reloading = false;
    bool fireHeld = false;
    float nextShotTime = 0f;

    void Awake()
    {
        bulletsLeft = magazineSize;
        if (!mainCamera) mainCamera = Camera.main;
    }

    void Update()
    {
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
        if (reloading) return;
        if (!readyToShoot) return;
        if (Time.time < nextShotTime) return;

        if (bulletsLeft <= 0) return;

        if (!mainCamera)
        {
            Debug.LogError("SingleWeaponController: mainCamera not set");
            return;
        }

        readyToShoot = false;


        // Step 1: Raycast from camera through mouse to get mouse in 3d space.
        Ray camRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(camRay, out RaycastHit camHit, Mathf.Infinity))
        {
            // Step 2: Set target position for bullet raycast.
            Vector3 targetPos = new Vector3(camHit.point.x, transform.position.y, camHit.point.z);
            // Step 3: Determine bullet's direction.
            Vector3 direction = (targetPos - transform.position).normalized;
            // Step 4: Apply spread.
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                Quaternion spreadRot = Quaternion.AngleAxis(yaw, Vector3.up);
                direction = spreadRot * direction;
            }
            // Step 5: Raycast from gun towards direction
            if (Physics.Raycast(transform.position, direction, out RaycastHit enemyHit, range, targetMask))
            {
                //Step 6: Apply damage if hit enemy
                Health enemyHealth = enemyHit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"Hit {enemyHit.collider.name} for {damage} damage. Health: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");
                }
                else
                {
                    Debug.Log("Hit: " + enemyHit.collider.name + " (no Health component)");
                }
            }

            // Ammo 
            bulletsLeft--;
            float secondsPerShot = 1f / Mathf.Max(0.0001f, fireRate);
            nextShotTime = Time.time + secondsPerShot;
            Invoke(nameof(ResetShot), secondsPerShot * 0.9f);

            // Burst continuation for semi-auto taps
            shotsRemainingThisTap = Mathf.Max(0, shotsRemainingThisTap - 1);
            if (!isAutomatic && shotsRemainingThisTap > 0 && bulletsLeft > 0)
                Invoke(nameof(TryShootOnce), secondsPerShot);
        }
        else
        {
            readyToShoot = true;
            Debug.LogWarning("Could not raycast from camera to mouse.");
            return;
        }
        
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

        // reset gates
        reloading = false;
        readyToShoot = true;
        nextShotTime = 0f;
        fireHeld = false;
    }
}