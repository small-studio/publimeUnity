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

        Light light = prefab.AddComponent<Light>();

        // Light type
        string type = root.SelectSingleNode("Type").InnerText;
        if (type == "POINT")
        {
            light.type = LightType.Point;
        }
        else if (type == "SPOT")
        {
            float spotSize = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("SpotSize").InnerText);
            light.spotAngle = spotSize;
            light.type = LightType.Spot;
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
        Color color = SmallParserUtils.ParseColorXml(root.SelectSingleNode("Color").InnerText);
        light.color = color;

        float power = SmallParserUtils.ParseFloatXml(root.SelectSingleNode("Power").InnerText);
        light.intensity = power;
        light.range = power / 3.0f;

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
        PrefabUtility.UnloadPrefabContents(prefab);
    }
}

}