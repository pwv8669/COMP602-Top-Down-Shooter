using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    public float Speed = 5f;
    private Vector2 moveInput;
    
    // Speed boost variables
    private float originalSpeed;
    private bool isSpeedBoostActive = false;
    private float speedBoostTimeRemaining = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalSpeed = Speed; // Store the original speed
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if(move.magnitude > 1f) move.Normalize();

        characterController.Move(move * Time.deltaTime * Speed);

        // Handle the countdown for the speed boost
        if (isSpeedBoostActive)
        {
            speedBoostTimeRemaining -= Time.deltaTime;

            if (speedBoostTimeRemaining <= 0)
            {
                // When time runs out, revert the speed
                DeactivateSpeedBoost();
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // This public method is called by the SpeedPotion script when collected
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        // Apply the speed boost
        Speed = originalSpeed * multiplier;
        speedBoostTimeRemaining = duration;
        isSpeedBoostActive = true;

        Debug.Log("Speed Boost Activated! Current speed: " + Speed);
    }

    private void DeactivateSpeedBoost()
    {
        // Revert to original speed
        Speed = originalSpeed;
        speedBoostTimeRemaining = 0f;
        isSpeedBoostActive = false;

        Debug.Log("Speed Boost Deactivated. Speed back to: " + Speed);
    }
}