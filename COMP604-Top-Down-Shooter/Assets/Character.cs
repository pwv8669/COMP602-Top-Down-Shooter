using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    private PhotonView photonView;
    public float Speed = 5f;
    private Vector2 moveInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only allow input for local player
        if (photonView != null && !photonView.IsMine)
            return;

        Vector3 move = new Vector3 (moveInput.x, 0, moveInput.y);

        if(move.magnitude > 1f) move.Normalize();

        characterController.Move(move*Time.deltaTime*Speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Only allow input for local player
        if (photonView != null && !photonView.IsMine)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

}
