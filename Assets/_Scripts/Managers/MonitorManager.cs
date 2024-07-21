using UnityEngine;

public class MonitorManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Connected displays: " + Display.displays.Length);
    
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log($"Display {i}: {Display.displays[i].systemWidth} x {Display.displays[i].systemHeight}");
        }
    }
}