using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun; // Soyun Edit this part - for PhotonView and muliplayer support
using UnityEngine.EventSystems; // Soyun add this part

public class PlayerLookAtMouse : MonoBehaviour
{
    public Camera mainCamera;
    private PhotonView photonView; // Soyun Edit this part

    //Soyun Edit this part
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Only set camera for local player
        if (photonView != null && photonView.IsMine)
        {
            mainCamera = Camera.main;
        }
    }
    void Update()
    {
        // Soyun Edit this part - Only control if this is MY player
        if (photonView != null && !photonView.IsMine)
            return;

        if (mainCamera == null)
            return;

        // Soyun Edit this part - Don't rotate when mouse is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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
