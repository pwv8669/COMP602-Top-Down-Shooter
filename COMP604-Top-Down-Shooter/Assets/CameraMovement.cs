using UnityEngine;
using Photon.Pun;  // Soyun add Photon for multiplayer support

public class CameraMovement : MonoBehaviour
{
    public Transform playerTarget;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 10f, 0f);

    // Soyun add variables for searching the local player
    private float searchInterval = 0.5f;
    private float lastSearchTime;

    // Soyun add method to find the local player

    void Start()
    {
        if (playerTarget == null)
        {
            FindMyPlayer();
            return;
        }
    }
    

    void LateUpdate()
    {
        // Soyun add method to find the local player in multiplayer
        if (playerTarget == null)
        {
            if (Time.time - lastSearchTime > searchInterval)
            {
                FindMyPlayer();
                lastSearchTime = Time.time;
            }
            return;
        }

        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Soyun add method to find the local player in multiplayer
    private void FindMyPlayer()
    {
        // Find Object what has PhotonView
        PhotonView[] photonViews = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);

        foreach (PhotonView pv in photonViews)
        {
            // Find my character
            if (pv.IsMine)
            {
                // Check for Character component
                Character character = pv.GetComponent<Character>();
                if (character != null)
                {
                    playerTarget = pv.transform;
                    Debug.Log("[CameraMovement] Found my player: " + pv.name);
                    break;
                }
            }
        }

        // Single player mode: find by "Player" tag if no PhotonView
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log("[CameraMovement] Found player by tag (single player mode)");
            }
        }
    }
} 

