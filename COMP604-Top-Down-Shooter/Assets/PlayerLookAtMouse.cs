using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAtMouse : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        // Get mouse position on screen.
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        // Create a ray from the camera to the mouse.
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        // If a raycast is successful.
        if(Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 targetPosition = hit.point; 

            // Get direction from player to mouse and set y to 0.
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            // Apply rotation.
            if(direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
            }
        }
    }
}
