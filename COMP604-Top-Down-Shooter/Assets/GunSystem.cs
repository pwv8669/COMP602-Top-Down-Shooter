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

    [Header("Audio")]
    public AudioSource audioSource;         // assign in inspector
    public AudioClip audioClip;           // assign in inspector

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
        Debug.Log("=== SHOOT ATTEMPT ===");
        
        if (reloading) { Debug.Log("Can't shoot: reloading"); return; }
        if (!readyToShoot) { Debug.Log("Can't shoot: not ready"); return; }
        if (Time.time < nextShotTime) { Debug.Log("Can't shoot: cooldown"); return; }
        if (bulletsLeft <= 0) { Debug.Log("Can't shoot: out of ammo"); return; }
        if (!mainCamera) { Debug.LogError("No main camera"); return; }

        readyToShoot = false;
        Debug.Log("Starting shot sequence...");

        // Step 1: Raycast from camera through mouse
        Ray camRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.Log($"Camera ray: {camRay.origin} -> {camRay.direction}");
        
        if (Physics.Raycast(camRay, out RaycastHit camHit, Mathf.Infinity))
        {
            Debug.Log($"Camera hit: {camHit.collider.name} at {camHit.point}");
            
            // Step 2: Set target position for bullet raycast
            Vector3 targetPos = new Vector3(camHit.point.x, transform.position.y, camHit.point.z);
            Debug.Log($"Target position: {targetPos}");
            
            // Step 3: Calculate direction from gun to target point
            Vector3 direction = (targetPos - transform.position).normalized;    
            direction.y = 0; // Keep it horizontal
            direction = direction.normalized;

            Debug.Log($"Using camera direction: {direction}");
            
            // Step 4: Apply spread
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                Quaternion spreadRot = Quaternion.AngleAxis(yaw, Vector3.up);
                direction = spreadRot * direction;
                Debug.Log($"After spread: {direction}");
            }
            
            // Visual debug ray
            Debug.DrawRay(transform.position, direction * range, Color.red, 2f);
            
            // Step 5: Raycast from gun towards direction
            Vector3 shootOrigin = new Vector3(transform.position.x, 1.0f, transform.position.z); // Character height

            Debug.Log($"Shooting from {shootOrigin} with range {range}");
            
            if (Physics.Raycast(shootOrigin, direction, out RaycastHit enemyHit, range, targetMask))
            {
                Debug.Log($"HIT ENEMY: {enemyHit.collider.name}");
                Debug.Log($"Hit point: {enemyHit.point}");
                Debug.Log($"Enemy layer: {enemyHit.collider.gameObject.layer}");
                
                Health enemyHealth = enemyHit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    Debug.Log($"Applying {damage} damage to enemy");
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"Enemy health now: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");
                }
                else
                {
                    Debug.LogWarning($"No Health component on {enemyHit.collider.name}");
                }
            }
            else
            {
                Debug.Log("Gun raycast MISSED enemy");
                Debug.Log("Check: Enemy layer, Collider, Range, Position");
            }

            if(audioSource && audioClip)
            {
                audioSource.PlayOneShot(audioClip);
            }

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
            Debug.LogWarning("Could not raycast from camera to mouse");
            readyToShoot = true;
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