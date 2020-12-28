using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Globalization;

namespace SUBlime
{

class CameraImporter : SUBlime.IAssetImporter
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

        Camera camera = prefab.AddComponent<Camera>();

        string projection = root.SelectSingleNode("Projection").InnerText;
        camera.orthographic = (projection != "PERSP");

        float fov = float.Parse(root.SelectSingleNode("Fov").InnerText, CultureInfo.InvariantCulture);
        camera.fieldOfView = (Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f) / camera.aspect) * 2.0f) * Mathf.Rad2Deg;

        float near = float.Parse(root.SelectSingleNode("Near").InnerText, CultureInfo.InvariantCulture);
        camera.nearClipPlane = near;

        float far = float.Parse(root.SelectSingleNode("Far").InnerText, CultureInfo.InvariantCulture);
        camera.farClipPlane = far;

        float size = float.Parse(root.SelectSingleNode("Size").InnerText, CultureInfo.InvariantCulture);
        camera.orthographicSize = size * 0.28f;

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