using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetFolderTreePrinter : MonoBehaviour
{
    /*
    [MenuItem("Tools/Print Asset Folder Tree")]
    static void PrintAssetFolderTree()
    {
        string filePath = Application.dataPath + "/../Assets/Metadata/AssetFolderTree.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string assetsPath = Application.dataPath;
            PrintDirectory(assetsPath, "", writer);
        }

        Debug.Log("Asset folder tree saved to: " + filePath);
    }

    static void PrintDirectory(string dir, string indent, StreamWriter writer)
    {
        foreach (string file in Directory.GetFiles(dir))
        {
            writer.WriteLine(indent + Path.GetFileName(file));
        }

        foreach (string subDir in Directory.GetDirectories(dir))
        {
            writer.WriteLine(indent + Path.GetFileName(subDir));
            PrintDirectory(subDir, indent + "  ", writer);
        }
    }
    */
}