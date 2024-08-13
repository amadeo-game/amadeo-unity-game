using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetFolderTreePrinter : MonoBehaviour
{
    [MenuItem("Tools/Print Asset Folder Tree")]
    static void PrintAssetFolderTree()
    {
        string assetsPath = Application.dataPath;
        PrintDirectory(assetsPath, "");
    }

    static void PrintDirectory(string dir, string indent)
    {
        foreach (string file in Directory.GetFiles(dir))
        {
            Debug.Log(indent + Path.GetFileName(file));
        }

        foreach (string subDir in Directory.GetDirectories(dir))
        {
            Debug.Log(indent + Path.GetFileName(subDir));
            PrintDirectory(subDir, indent + "  ");
        }
    }
}