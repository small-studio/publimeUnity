using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class SceneImporter : IAssetImporter
{
    void UpdateScenePrefab(string xmlPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);
        XmlNode root = doc.DocumentElement;

        string path = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string fullPath = Path.Combine(path, fileName + ".prefab");

        // Load the prefab asset
        GameObject scenePrefab = PrefabUtility.LoadPrefabContents(fullPath);
        if (scenePrefab == null)
        {
            Debug.LogWarning("[SceneImporter] There is no prefab at path " + fullPath);
            return;
        }

        Dictionary<string, KeyValuePair<string, GameObject>> relations = new Dictionary<string, KeyValuePair<string, GameObject>>();
        XmlNodeList objects = root.SelectSingleNode("Objects").ChildNodes;
        foreach (XmlNode xmlObject in objects)
        {
            GameObject gameObject = null;
            if (xmlObject.Name == "Empty")
            {
                gameObject = new GameObject();
            }
            else if (xmlObject.Name == "Object")
            {
                string prefabPath = xmlObject.SelectSingleNode("Prefab").InnerText;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (gameObject != null)
                {
                    string isStatic = xmlObject.SelectSingleNode("Static").InnerText;
                    gameObject.isStatic = (isStatic == "Static");

                    // Check if Layer is Valid and Set
                    string layer = xmlObject.SelectSingleNode("Layer").InnerText;
                    if (layer != "")
                    {
                        int layeridx = LayerMask.NameToLayer(layer);
                        gameObject.layer = ((layeridx >= 0) ? layeridx : 0);
                    }

                    // Check if Tag is Valid and Set
                    string tag = xmlObject.SelectSingleNode("Tag").InnerText;
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
                else
                {
                    Debug.LogWarning("Could not instantiate prefab at path " + prefabPath);
                }
            }

            if (gameObject != null)
            {
                // Apply transform
                string location = xmlObject.SelectSingleNode("Position").InnerText;
                string rotation = xmlObject.SelectSingleNode("Rotation").InnerText;
                string scale = xmlObject.SelectSingleNode("Scale").InnerText;
                string name = xmlObject.SelectSingleNode("Name").InnerText;

                gameObject.name = name;
                gameObject.transform.parent = scenePrefab.transform;

                gameObject.transform.position = SmallImporterUtils.ParseVectorXml(location);
                gameObject.transform.localScale = SmallImporterUtils.ParseVectorXml(scale);
                Vector3 rotationVector = SmallImporterUtils.ParseVectorXml(rotation);

                gameObject.transform.rotation = new Quaternion();
                gameObject.transform.Rotate(new Vector3(rotationVector[0] * -1, 0, 0), Space.World);
                gameObject.transform.Rotate(new Vector3(0, rotationVector[2] * -1, 0), Space.World);
                gameObject.transform.Rotate(new Vector3(0, 0, rotationVector[1] * -1), Space.World);

                relations.Add(name, new KeyValuePair<string, GameObject>(xmlObject.SelectSingleNode("Parent").InnerText, gameObject));
            }
        }

        // Link childs to parents
        foreach (KeyValuePair<string, KeyValuePair<string, GameObject>> relation in relations)
        {
            string parentName = relation.Value.Key;
            GameObject gameObject = relation.Value.Value;

            if (relations.ContainsKey(parentName))
            {
                GameObject parent = relations[parentName].Value;
                gameObject.transform.parent = parent.transform;
            }
        }

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(scenePrefab, fullPath);
        PrefabUtility.UnloadPrefabContents(scenePrefab);
    }

    public void OnPreImport(string assetPath, AssetImporter importer)
    {
        SmallImporterUtils.CreatePrefabXml(assetPath);
    }

    public void OnPostImport(string assetPath)
    {
        UpdateScenePrefab(assetPath);
    }
}

}