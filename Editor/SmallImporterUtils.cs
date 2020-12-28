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
                    else if (type == "TRANSPARENT")
                    {
                        textureImporter.alphaIsTransparency = true;
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
        sourceString = sourceString.Substring(8, sourceString.Length - 9); // Remove parenthesis
        string[] splitString = sourceString.Split(',');

        Vector3 value = Vector3.zero;
        value.x = float.Parse(splitString[0], CultureInfo.InvariantCulture);
        value.y = float.Parse(splitString[1], CultureInfo.InvariantCulture);
        value.z = float.Parse(splitString[2], CultureInfo.InvariantCulture);

        return value;
    }

    public static void RecursiveParseTransformXml(XmlNode root, GameObject parent)
    {
        XmlNode childrenNode = root.SelectSingleNode("Children");
        if (childrenNode != null)
        {
            XmlNodeList childrenNodeList = childrenNode.ChildNodes;
            for (int i = 0; i < childrenNodeList.Count; i++)
            {
                string type = childrenNodeList[i].SelectSingleNode("Type").InnerText;
                GameObject gameObject = null;
                if (type == "MESH")
                {
                    string path = childrenNodeList[i].SelectSingleNode("Prefab").InnerText;
                    GameObject child = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                    gameObject = PrefabUtility.InstantiatePrefab(child) as GameObject;

                    if (gameObject != null)
                    {
                        string isStatic = childrenNodeList[i].SelectSingleNode("Static").InnerText;
                        gameObject.isStatic = (isStatic == "Static");

                        // Check if Layer is Valid and Set
                        string layer = childrenNodeList[i].SelectSingleNode("Layer").InnerText;
                        if (layer != "")
                        {
                            int layeridx = LayerMask.NameToLayer(layer);
                            gameObject.layer = ((layeridx >= 0) ? layeridx : 0);
                        }

                        // Check if Tag is Valid and Set
                        string tag = childrenNodeList[i].SelectSingleNode("Tag").InnerText;
                        if (tag != "")
                        {
                            for (int j = 0; j < UnityEditorInternal.InternalEditorUtility.tags.Length; j++)
                            {
                                if (UnityEditorInternal.InternalEditorUtility.tags[j].Contains(tag))
                                {
                                    gameObject.tag = tag;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (type == "LIGHT" || type == "CAMERA")
                {
                    string path = childrenNodeList[i].SelectSingleNode("Prefab").InnerText;
                    GameObject child = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                    gameObject = PrefabUtility.InstantiatePrefab(child) as GameObject;
                }
                else if (type == "EMPTY")
                {
                    gameObject = new GameObject();
                }

                if (gameObject != null)
                {
                    SmallImporterUtils.ParseTransformXml(childrenNodeList[i], gameObject, type);
                    gameObject.GetComponent<Transform>().SetParent(parent.GetComponent<Transform>(), false);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject.GetComponent<Transform>());

                    SmallImporterUtils.RecursiveParseTransformXml(childrenNodeList[i], gameObject);
                }
            }
        }
    }

    public static void ParseTransformXml(XmlNode node, GameObject gameObject, string type)
    {
        string location = node.SelectSingleNode("Position").InnerText;
        string rotation = node.SelectSingleNode("Rotation").InnerText;
        string scale = node.SelectSingleNode("Scale").InnerText;
        string name = node.SelectSingleNode("Name").InnerText;

        gameObject.name = name;
        gameObject.transform.localPosition = SmallImporterUtils.ParseVectorXml(location);
        gameObject.transform.localScale = SmallImporterUtils.ParseVectorXml(scale);

        Vector3 rotationVector = SmallImporterUtils.ParseVectorXml(rotation);
        gameObject.transform.rotation = new Quaternion();
        gameObject.transform.Rotate(new Vector3(rotationVector[0] * -1, 0, 0), Space.World);
        gameObject.transform.Rotate(new Vector3(0, rotationVector[2] * -1, 0), Space.World);
        gameObject.transform.Rotate(new Vector3(0, 0, rotationVector[1] * -1), Space.World);
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