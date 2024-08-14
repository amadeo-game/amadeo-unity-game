using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptReferenceFinder : MonoBehaviour
{
    [MenuItem("Tools/Find All Script References")]
    static void FindScriptReferences()
    {
        string filePath = Application.dataPath + "/../Assets/Metadata/ScriptReferences.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            MonoBehaviour[] allScripts = GameObject.FindObjectsOfType<MonoBehaviour>();
            foreach (var script in allScripts)
            {
                string output = script.GetType().Name + " attached to " + script.gameObject.name;
                writer.WriteLine(output);
            }
        }

        Debug.Log("Script references saved to: " + filePath);
    }
}