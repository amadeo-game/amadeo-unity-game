using UnityEngine;
using UnityEditor;

public class ScriptReferenceFinder : MonoBehaviour
{
    [MenuItem("Tools/Find All Script References")]
    static void FindScriptReferences()
    {
        MonoBehaviour[] allScripts = GameObject.FindObjectsOfType<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            Debug.Log(script.GetType().Name + " attached to " + script.gameObject.name);
        }
    }
}