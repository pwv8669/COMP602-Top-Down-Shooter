using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    public float Speed = 5f;
    private Vector2 moveInput;

    private PhotonView pv; //Soyun add this part

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        pv = GetComponent<PhotonView>(); //Soyun add this part
    }

    // Update is called once per frame
    void Update()
    {
        if (pv != null && !pv.IsMine) return; //Soyun add this part

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude > 1f) move.Normalize();

        characterController.Move(move * Time.deltaTime * Speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // If not my character, ignore input - Soyun add this part
        if (pv != null && !pv.IsMine) return;

        moveInput = context.ReadValue<Vector2>();
    }

}
