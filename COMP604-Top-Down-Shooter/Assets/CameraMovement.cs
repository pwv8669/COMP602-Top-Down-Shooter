using UnityEngine;
using Photon.Pun; //Soyun edit this part

public class CameraMovement : MonoBehaviour
{
    public Transform playerTarget;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 10f, 0f);

    //Soyun edit this part
    void Start()
    {
        // Find the local player automatically
        FindLocalPlayer();
    }
    void FindLocalPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                playerTarget = player.transform;
                Debug.Log("Camera following local player: " + player.name);
                return;
            }
        }

        // If not found, try again in a moment
        if (playerTarget == null)
        {
            Invoke(nameof(FindLocalPlayer), 0.5f);
        }
    }
    //Edit till here

    void LateUpdate()
    {
        if (playerTarget == null)
            return;
            
        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
} 

