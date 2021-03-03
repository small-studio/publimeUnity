using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class PrefabImporter : AAssetImporter
{
    public override void CreateDependencies(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

        // Add materials dependencies
        XmlNodeList materialsNode = root.SelectSingleNode("Materials").ChildNodes;
        for (int i = 0; i < materialsNode.Count; i++)
        {
            string materialPath = materialsNode[i].SelectSingleNode("Path").InnerText;
            string name = materialsNode[i].SelectSingleNode("Name").InnerText;
            string fullPath = Path.Combine(materialPath, name + ".mat");
            AddDependency<Material>(fullPath);
        }

        // Add mesh dependency
        string meshPath = root.SelectSingleNode("Model").InnerText + ".fbx";
        AddDependency<Mesh>(meshPath);

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
        if (prefab == null)
        {
            Debug.LogWarning("[PrefabImporter] There is no prefab at path " + fullPath);
            return;
        }
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Load and assign the mesh
        string meshPath = root.SelectSingleNode("Model").InnerText + ".fbx";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        MeshFilter meshFilter = prefabInstance.GetOrAddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);

        // Load and assign materials
        MeshRenderer renderer = prefabInstance.GetOrAddComponent<MeshRenderer>();
        XmlNodeList materialsNode = root.SelectSingleNode("Materials").ChildNodes;
        if (materialsNode.Count != 0)
        {
            Material[] materials = new Material[materialsNode.Count];
            for (int i = 0; i < materialsNode.Count; i++)
            {
                string path = materialsNode[i].SelectSingleNode("Path").InnerText;
                string name = materialsNode[i].SelectSingleNode("Name").InnerText;
                string materialPath = Path.Combine(path, name + ".mat");

                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }

            renderer.sharedMaterials = materials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
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