using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Globalization;
using System.IO;

namespace SUBlime
{

public class SmallImporterUtils
{
    public const string SMALL_PREFAB_EXTENSION = ".subp";
    public const string SMALL_MATERIAL_EXTENSION = ".subm";
    public const string SMALL_SCENE_EXTENSION = ".subs";
    public const string SMALL_LIGHT_EXTENSION = ".subl";
    public const string SMALL_CAMERA_EXTENSION = ".subc";

    public static Texture ParseTextureXml(string sourceString)
    {
        Texture texture = null;
        string[] splitString = sourceString.Split(',');
        if (splitString.Length > 2)
        {
            string type = splitString[1];
            string path = splitString[2];
            if (path != null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    if (type == "NORMAL")
                    {
                        textureImporter.textureType = TextureImporterType.NormalMap;
                    }
                    AssetDatabase.ImportAsset(path);
                    texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                }
            }
        }
        return texture;
    }

    public static float ParseFloatXml(string sourceString)
    {
        float value = 0.0f;
        string[] splitString = sourceString.Split(',');
        if (splitString.Length > 1)
        {
            value = float.Parse(splitString[1], CultureInfo.InvariantCulture);
        }
        return value;
    }

    public static Color ParseColorXml(string sourceString)
    {
        Color color = Color.white;
        string[] splitString = sourceString.Split(',');
        if (splitString.Length > 1)
        {
            string hex = splitString[1];
            ColorUtility.TryParseHtmlString(hex, out color);
        }
        return color;
    }

    
    public static Vector3 ParseVectorXml(string sourceString)
    {
        sourceString = sourceString.Substring(1, sourceString.Length - 2); // Remove parenthesis
        string[] splitString = sourceString.Split(',');

        Vector3 value = Vector3.zero;
        value.x = float.Parse(splitString[0], CultureInfo.InvariantCulture);
        value.y = float.Parse(splitString[1], CultureInfo.InvariantCulture);
        value.z = float.Parse(splitString[2], CultureInfo.InvariantCulture);

        return value;
    }

    public static void CreatePrefabXml(string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        string path = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string fullPath = Path.Combine(path, fileName + ".prefab");
        
        GameObject gameObject = new GameObject();
        gameObject.name = fileName;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        PrefabUtility.SaveAsPrefabAsset(gameObject, Path.Combine(path, fileName + ".prefab"));
        GameObject.DestroyImmediate(gameObject);
    }
}

}