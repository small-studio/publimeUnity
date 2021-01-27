using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class CameraImporter : AAssetImporter
{
    public override void CreateDependencies(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

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
        GameObject prefab = PrefabUtility.LoadPrefabContents(fullPath);
        if (prefab == null)
        {
            Debug.LogWarning("[PrefabImporter] There is no prefab at path " + fullPath);
            return;
        }

        Camera camera = prefab.AddComponent<Camera>();

        string projection = root.SelectSingleNode("Projection").InnerText;
        camera.orthographic = (projection != "PERSP");

        float fov = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Fov").InnerText);
        camera.fieldOfView = (Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f) / camera.aspect) * 2.0f) * Mathf.Rad2Deg;

        float near = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Near").InnerText);
        camera.nearClipPlane = near;

        float far = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Far").InnerText);
        camera.farClipPlane = far;

        float size = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Size").InnerText);
        camera.orthographicSize = size * 0.28f;

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
        PrefabUtility.UnloadPrefabContents(prefab);
    }
}

}