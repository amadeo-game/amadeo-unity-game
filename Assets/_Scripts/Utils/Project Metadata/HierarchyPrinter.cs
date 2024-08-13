using UnityEngine;
using UnityEditor;

public class HierarchyPrinter : MonoBehaviour
{
    [MenuItem("Tools/Print Scene Hierarchy")]
    static void PrintHierarchy()
    {
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            PrintHierarchyRecursive(obj, "");
        }
    }

    static void PrintHierarchyRecursive(GameObject obj, string indent)
    {
        Debug.Log(indent + obj.name);
        foreach (Transform child in obj.transform)
        {
            PrintHierarchyRecursive(child.gameObject, indent + "  ");
        }
    }
}
