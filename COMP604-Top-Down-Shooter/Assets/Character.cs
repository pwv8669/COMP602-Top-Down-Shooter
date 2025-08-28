using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    public float Speed = 5f;
    private Vector2 moveInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3 (moveInput.x, 0, moveInput.y);

        if(move.magnitude > 1f) move.Normalize();

        characterController.Move(move*Time.deltaTime*Speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

}
