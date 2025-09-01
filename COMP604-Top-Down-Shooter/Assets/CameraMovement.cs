using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform playerTarget;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 10f, 0f);
    
    void LateUpdate()
    {
        if (playerTarget == null)
            return;
            
        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(playerTarget);
    }
}

