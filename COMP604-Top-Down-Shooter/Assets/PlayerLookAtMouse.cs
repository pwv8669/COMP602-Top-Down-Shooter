using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun; // Soyun add Photon for multiplayer support

public class PlayerLookAtMouse : MonoBehaviour
{
    public Camera mainCamera;
    private PhotonView pv; // Soyun add for PhotonView reference

    // Soyun add Start method to get PhotonView
    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (pv != null && !pv.IsMine) return; // Soyun add to ensure only local player can control their character

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
