using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun; // Soyun add Photon for multiplayer support

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    public float Speed = 5f;
    private Vector2 moveInput;

    private PhotonView pv; // Soyun add for PhotonView reference


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        pv = GetComponent<PhotonView>(); // Soyun add for PhotonView
    }

    // Update is called once per frame
    void Update()
    {
        if (pv != null && !pv.IsMine) return; // Soyun add to ensure only local player can control their character

        Vector3 move = new Vector3 (moveInput.x, 0, moveInput.y);

        if(move.magnitude > 1f) move.Normalize();

        characterController.Move(move*Time.deltaTime*Speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (pv != null && !pv.IsMine) return; // Soyun add to ensure only local player can control their character

        moveInput = context.ReadValue<Vector2>();
    }

}
