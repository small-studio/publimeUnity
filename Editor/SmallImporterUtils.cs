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

    public static string GetTexturePath(string sourceString)
    {
        string[] splitString = sourceString.Split(',');
        if (splitString.Length > 2)
        {
            return splitString[2];
        }
        return null;
    }

    public static bool PrefabExists(string fileName, string path)
    {
        string[] results = AssetDatabase.FindAssets(fileName, new string[] { path });
        if (results != null && results.Length > 0)
        {
            foreach (var result in results)
            {
                string assetName = Path.GetFileName(AssetDatabase.GUIDToAssetPath(result));
                if (assetName == fileName + ".prefab")
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void CreatePrefabFromXml(string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        string path = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string fullPath = Path.Combine(path, fileName + ".prefab");
        
        if (PrefabExists(fullPath, path))
        {
            SmallLogger.Log(SmallLogger.LogType.PreImport, "Prefab '" + fileName + "' already exists.");
        }
        else
        {
            GameObject prefab = new GameObject();
            prefab.name = fileName;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            SmallLogger.Log(SmallLogger.LogType.PreImport, "Creating prefab '" + fileName + "' from xml '" + xmlPath + "'");
            
            PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
            GameObject.DestroyImmediate(prefab);
        }

        // Update the created asset to ensure we trigger the 'OnPostprocessAllAssets' method
        AssetDatabase.ImportAsset(fullPath);
    }

    public static Shader GetShaderFromName(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            SmallLogger.LogWarning(SmallLogger.LogType.PreImport, "The shader \"" + shaderName + "\" does not exists.");
        }
        return shader;
    }

    public static Material CreateMaterialFromXml(string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string path = Path.Combine(root.SelectSingleNode("Path").InnerText, fileName + ".mat");

        // Try to set the shader
        XmlNode shaderNode = root.SelectSingleNode("Shader");
        string shaderName = SmallImporterWindow.defaultShader != null ? SmallImporterWindow.defaultShader.name : "Standard";
        if (shaderNode != null)
        {
            shaderName = root.SelectSingleNode("Shader").InnerText;
        }
        else
        {
            SmallLogger.LogWarning(SmallLogger.LogType.PreImport, "Shader name not found in material '" + fileName + "'. The material is probably missing a group node.");
        }

        Shader shader = GetShaderFromName(shaderName);

        // If the material does not exists, we create it
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        return material;
    }

    public static void RecursiveGetTransformDependecies(AAssetImporter importer, XmlNode root)
    {
        XmlNode childrenNode = root.SelectSingleNode("Children");
        if (childrenNode != null)
        {
            XmlNodeList childrenNodeList = childrenNode.ChildNodes;
            for (int i = 0; i < childrenNodeList.Count; i++)
            {
                string type = childrenNodeList[i].SelectSingleNode("Type").InnerText;
                if (type == "MESH" || type == "LIGHT" || type == "CAMERA")
                {
                    string path = childrenNodeList[i].SelectSingleNode("Prefab").InnerText;
                    importer.AddDependency<GameObject>(path);
                }

                SmallImporterUtils.RecursiveGetTransformDependecies(importer, childrenNodeList[i]);
            }
        }
    }
}

}