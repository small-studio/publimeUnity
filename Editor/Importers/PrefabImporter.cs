using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class PrefabImporter : SUBlime.IAssetImporter
{
    void UpdatePrefab(string xmlPath)
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
            Debug.LogWarning("[PrefabImporter] There is no prefab at path " + fullPath);
            return;
        }

        // Load and assign the mesh
        string meshPath = root.SelectSingleNode("Model").InnerText + ".fbx";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Load and assign materials
        XmlNodeList materialsNode = root.SelectSingleNode("Materials").ChildNodes;
        Material[] materials = new Material[materialsNode.Count];
        for (int i = 0; i < materialsNode.Count; i++)
        {
            string path = materialsNode[i].SelectSingleNode("Path").InnerText;
            string name = materialsNode[i].SelectSingleNode("Name").InnerText;
            string materialPath = Path.Combine(path, name + ".mat");

            materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        }

        MeshRenderer renderer = prefab.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = materials;

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
        UpdatePrefab(assetPath);
    }
}

}