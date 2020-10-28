using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class MaterialImporter : IAssetImporter
{
    Material _material = null;

    Shader GetShaderFromName(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            if (shaderName == "Custom")
            {
                string shaderPath = EditorPrefs.GetString("SBI_customShader", "");
                shader = Shader.Find(shaderPath);
            }

            if (shaderName == "Custom_Transparent")
            {
                string shaderPath = EditorPrefs.GetString("SBI_customShaderTransparent", "");
                shader = Shader.Find(shaderPath);
            }
        }
        return shader;
    }

    Material CreateMaterial(string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        string shaderName = root.SelectSingleNode("Shader").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string path = Path.Combine(root.SelectSingleNode("Path").InnerText, fileName + ".mat");

        Shader shader = GetShaderFromName(shaderName);
        if (shader == null)
        {
            Debug.LogWarning("[Small Importer] The material " + fileName + " has not a valid shader name.");
            // TODO use an error shader
        }

        // If the material does not exists, we create it
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            if (shader != null)
            {
                material.shader = shader;
            }
            AssetDatabase.CreateAsset(material, path);
            Debug.Log("[Small Importer] Creating new material from xml " + fileName + ".");
        }
        return material;
    }

    void UpdateMaterialParameters(Material material, string xmlPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);
        XmlNode root = xml.DocumentElement;

        // Set Keywords
        material.EnableKeyword("_NORMALMAP");
        material.EnableKeyword("_EMISSION");
        material.EnableKeyword("_METALLICGLOSSMAP");
        material.EnableKeyword("_SPECGLOSSMAP");

        // Get Data from Channels when available
        XmlNode channels = root.SelectSingleNode("Channels");

        // Base Color
        XmlNode colorXml = channels.SelectSingleNode("_Color");
        if (colorXml != null)
        {
            Color color = SmallImporterUtils.ParseColorXml(colorXml.InnerText);
            material.SetColor("_Color", color);
            material.SetColor("_BaseColor", color); // URP Unlit
            material.SetColor("_UnlitColor", color); // HDRP Unlit
        }

        // Albedo
        XmlNode mainTexXml = channels.SelectSingleNode("_MainTex");
        if (mainTexXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(mainTexXml.InnerText);
            material.SetTexture("_MainTex", texture);
            material.SetTexture("_BaseMap", texture); // URP Unlit
            material.SetTexture("_UnlitColorMap", texture); //HDRP Unlit
            material.SetFloat("_UseColorMap", texture != null ? 1.0f : 0.0f);
        }

        // Metallic
        XmlNode metallicGlossMapXml = channels.SelectSingleNode("_MetallicGlossMap");
        if (metallicGlossMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(metallicGlossMapXml.InnerText);
            if (texture)
            {
                material.SetTexture("_MetallicGlossMap", texture);
                material.SetFloat("_UseMetalicMap", 1.0f); // URP Autodesk
            }
            else
            {
                XmlNode metallicXml = channels.SelectSingleNode("_Metallic");
                if (metallicXml != null)
                {
                    float value = SmallImporterUtils.ParseFloatXml(metallicXml.InnerText);
                    material.SetTexture("_MetallicGlossMap", null);
                    material.SetFloat("_Metallic", value);
                    material.SetFloat("_UseMetalicMap", 0.0f); // URP Autodesk
                }
            }
        }

        // Roughness
        XmlNode specMapXml = channels.SelectSingleNode("_SpecGlossMap");
        if (specMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(specMapXml.InnerText);
            if (texture)
            {
                material.SetTexture("_SpecGlossMap", texture);
                material.SetFloat("_UseRoughnessMap", 1.0f); // URP Autodesk
            }
            else
            {
                XmlNode glossinessXml = channels.SelectSingleNode("_Glossiness");
                if (glossinessXml != null)
                {
                    float value = SmallImporterUtils.ParseFloatXml(glossinessXml.InnerText);
                    material.SetTexture("_SpecGlossMap", null);
                    material.SetFloat("_Glossiness", value);
                    material.SetFloat("_UseRoughnessMap", 0.0f); // URP Autodesk
                }
            }
        }

        // Normal Map
        XmlNode bumpMapXml = channels.SelectSingleNode("_BumpMap");
        if (bumpMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(bumpMapXml.InnerText);
            material.SetTexture("_BumpMap", texture);
        }

        // Emission
        XmlNode emissionMapXml = channels.SelectSingleNode("_EmissionMap");
        if (emissionMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(channels.SelectSingleNode("_EmissionMap").InnerText);
            if (texture)
            {
                material.SetTexture("_EmissionMap", texture);
                material.SetColor("_EmissionColor", Color.white);
            }
        }
        else
        {
            Color color = Color.white;
            XmlNode emissionColorXml = channels.SelectSingleNode("_EmissionColor");
            if (emissionColorXml != null)
            {
                color = SmallImporterUtils.ParseColorXml(emissionColorXml.InnerText);
            }
            material.SetColor("_EmissionColor", color);
            material.SetTexture("_EmissionMap", null);
        }

        // Tile and lightmap data
        XmlNode tileMaxXml = channels.SelectSingleNode("_TileMax");
        if (tileMaxXml != null)
        {
            float tileMax = SmallImporterUtils.ParseFloatXml(tileMaxXml.InnerText);
            float tileScale = 1.0f / tileMax;
            material.SetFloat("_TileScale", tileScale);

            float tileX = SmallImporterUtils.ParseFloatXml(channels.SelectSingleNode("_TileX").InnerText);
            float tileY = SmallImporterUtils.ParseFloatXml(channels.SelectSingleNode("_TileY").InnerText);
            material.SetVector("_Offset", new Vector4(tileX * tileScale, tileY * tileScale));
            
            // Lightmap
            XmlNode lightmapXml = channels.SelectSingleNode("_Lightmap");
            if (lightmapXml != null)
            {
                Texture texture = SmallImporterUtils.ParseTextureXml(lightmapXml.InnerText);
                material.SetTexture("_Lightmap", texture);
            }
        }

        // Transparent
        XmlNode transparentXml = channels.SelectSingleNode("_Transparent");
        if (transparentXml != null)
        {
            string value = transparentXml.InnerText;
            float mode = 0.0f;
            if (value == "OPAQUE")
            {
                mode = 0.0f;
            }
            else if (value == "CLIP")
            {
                mode = 1.0f;
            }
            else if (value == "BLEND")
            {
                mode = 2.0f;
            }
            material.SetFloat("_Mode", mode);
        }

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void OnPreImport(string assetPath, AssetImporter importer)
    {
        _material = CreateMaterial(assetPath);
    }

    public void OnPostImport(string assetPath)
    {
        // We must do this step on post import because we don't know in which order textures and materials are imported
        UpdateMaterialParameters(_material, assetPath);
    }
}

}