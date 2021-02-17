using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class SceneImporter : AAssetImporter
{
    public override void CreateDependencies(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

        string path = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(assetPath);

        // Add it's own prefab dependency
        AddDependency<GameObject>(Path.Combine(path, fileName + ".prefab"));

        // Add prefabs dependencies
        SmallImporterUtils.RecursiveGetTransformDependecies(this, root);
    }

    public override void OnPreImport(string assetPath)
    {
        SmallImporterUtils.CreatePrefabFromXml(assetPath);
    }

    public override void OnPostImport(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

        string prefabPath = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string fullPath = Path.Combine(prefabPath, fileName + ".prefab");

        // Load the prefab asset
        GameObject prefab = AssetDatabase.LoadMainAssetAtPath(fullPath) as GameObject;
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (prefabInstance == null)
        {
            Debug.LogWarning("[SceneImporter] There is no prefab at path " + fullPath);
            return;
        }

        // Load and set children
        SmallParserUtils.RecursiveParseTransformXml(root, prefabInstance);

        // Save prefab asset
        PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance.GetComponent<Transform>());
        PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);

        // Clean up
        GameObject.DestroyImmediate(prefabInstance);

        // Force Unity to update the asset, without this we have to manually reload unity (by losing and gaining focus on the editor)
        AssetDatabase.ImportAsset(fullPath);
    }
}

}