using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class SceneImporter : IAssetImporter
{
    void UpdateScenePrefab(string xmlPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);
        XmlNode root = doc.DocumentElement;

        string prefabPath = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string fullPath = Path.Combine(prefabPath, fileName + ".prefab");

        // Load the prefab asset
        GameObject prefab = PrefabUtility.LoadPrefabContents(fullPath);
        if (prefab == null)
        {
            Debug.LogWarning("[SceneImporter] There is no prefab at path " + fullPath);
            return;
        }

        // Load and set children
        SmallImporterUtils.RecursiveParseTransformXml(root, prefab);

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
        PrefabUtility.UnloadPrefabContents(prefab);
    }

    public void OnPreImport(string assetPath, AssetImporter importer)
    {
        SmallImporterUtils.CreatePrefabXml(assetPath);
    }

    public void OnPostImport(string assetPath)
    {
        UpdateScenePrefab(assetPath);
    }
}

}