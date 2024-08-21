using UnityEngine;

public class Debugging : MonoBehaviour
{
    void Start()
    {
        Debug.Log(SystemInfo.graphicsDeviceName);
    }
}