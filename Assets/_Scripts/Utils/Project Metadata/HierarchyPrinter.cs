using UnityEngine;
using UnityEditor;
using System.IO;

public class HierarchyPrinter : MonoBehaviour
{
    /*
    [MenuItem("Tools/Print Scene Hierarchy")]
    static void PrintHierarchy()
    {
        string filePath = Application.dataPath + "/../Assets/Metadata/SceneHierarchy.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                PrintHierarchyRecursive(obj, "", writer);
            }
        }

        Debug.Log("Scene hierarchy saved to: " + filePath);
    }

    static void PrintHierarchyRecursive(GameObject obj, string indent, StreamWriter writer)
    {
        writer.WriteLine(indent + obj.name);
        foreach (Transform child in obj.transform)
        {
            PrintHierarchyRecursive(child.gameObject, indent + "  ", writer);
        }
    }
    */
}