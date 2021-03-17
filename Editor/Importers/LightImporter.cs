using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class LightImporter : AAssetImporter
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
        if (prefab == null)
        {
            SmallLogger.LogWarning(SmallLogger.LogType.PostImport, "There is no prefab at path " + fullPath);
            return;
        }
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        Light light = prefabInstance.GetOrAddComponent<Light>();

        // Light type
        string type = root.SelectSingleNode("Type").InnerText;
        if (type == "POINT")
        {
            light.type = LightType.Point;
            light.range = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Radius").InnerText);
        }
        else if (type == "SPOT")
        {
            light.type = LightType.Spot;
            light.spotAngle = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("SpotSize").InnerText);
            light.range = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Radius").InnerText);
        }
        else if (type == "SUN")
        {
            light.type = LightType.Directional;
        }
        else if (type == "AREA")
        {
            string shape = root.SelectSingleNode("Shape").InnerText;
            if (shape == "RECTANGLE")
            {
                light.type = LightType.Rectangle;
                float width = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Width").InnerText);
                float height = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Height").InnerText);
                light.areaSize = new Vector2(width, height);
            }
            else if (shape == "DISC")
            {
                light.type = LightType.Disc;
                float radius = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Radius").InnerText);
                light.areaSize = new Vector2(radius, radius);
            }
        }

        // Light color
        light.color = SmallParserUtils.ParseColorXml(root.SelectSingleNode("Color").InnerText);
        light.intensity = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Power").InnerText) / 100.0f;

        // Save prefab asset
        PrefabUtility.RecordPrefabInstancePropertyModifications(light);
        PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);

        // Clean up
        GameObject.DestroyImmediate(prefabInstance);

        // Force Unity to update the asset, without this we have to manually reload unity (by losing and gaining focus on the editor)
        AssetDatabase.ImportAsset(fullPath);
    }
}

}