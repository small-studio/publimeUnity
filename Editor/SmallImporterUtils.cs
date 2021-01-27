﻿using UnityEngine;
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

    public static void CreatePrefabFromXml(string xmlPath)
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

    public static Shader GetShaderFromName(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogWarning("The shader \"" + shaderName + "\" does not exists.");
        }
        return shader;
    }

    public static Material CreateMaterialFromXml(string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        string shaderName = root.SelectSingleNode("Shader").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string path = Path.Combine(root.SelectSingleNode("Path").InnerText, fileName + ".mat");

        // If the material does not exists, we create it
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        
        Shader shader = GetShaderFromName(shaderName);
        if (shader != null)
        {
            material.shader = shader;
            Debug.Log("[Small Importer] Creating new material from xml '" + fileName + "' using shader '" + shaderName + "'");
        }
        else
        {
            Debug.Log("[Small Importer] Creating new material from xml '" + fileName + "'. Shader name is invalid '" + shaderName + "'");
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