using UnityEngine;

public class DebugPath : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Persistent Data Path is: " + Application.persistentDataPath);
    }
}
