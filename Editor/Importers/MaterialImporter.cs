using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;

namespace SUBlime
{

class MaterialImporter : AAssetImporter
{
    Material _material = null;

    public override void CreateDependencies(string assetPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(assetPath);
        XmlNode root = xml.DocumentElement;

        List<string> textures = new List<string>();
        textures.Add("_MainTex");
        textures.Add("_MetallicGlossMap");
        textures.Add("_SpecGlossMap");
        textures.Add("_BumpMap");
        textures.Add("_EmissionMap");
        textures.Add("_Lightmap");

        // TODO create function with other specific function for add dependency
        // Add textures dependencies
        XmlNode channels = root.SelectSingleNode("Channels");
        foreach (string texture in textures)
        {
            XmlNode node = channels.SelectSingleNode(texture);
            if (node != null)
            {
                AddDependency<Texture>(SmallImporterUtils.GetTexturePath(node.InnerText));
            }
        }
    }

    public override void OnPreImport(string assetPath)
    {
        _material = SmallImporterUtils.CreateMaterialFromXml(assetPath);
    }

    public override void OnPostImport(string assetPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(assetPath);
        XmlNode root = xml.DocumentElement;

        // Get Data from Channels when available
        XmlNode channels = root.SelectSingleNode("Channels");

        // Custom parameters
        XmlNode custom = channels.SelectSingleNode("Custom");
        if (custom != null)
        {
            XmlNode name = custom.SelectSingleNode("Name");
            if (name != null)
            {
                Shader shader = SmallImporterUtils.GetShaderFromName(name.InnerText);
                if (shader != null)
                {
                    _material.shader = shader;
                    foreach (XmlNode node in custom.ChildNodes)
                    {
                        ParseFactory.Parse(node.InnerText, node.Name, _material);
                    }
                }
            }
        }

        // Base Color
        XmlNode colorXml = channels.SelectSingleNode("_Color");
        if (colorXml != null)
        {
            Color color = SmallImporterUtils.ParseColorXml(colorXml.InnerText);
            _material.SetColor("_Color", color);
            _material.SetColor("_BaseColor", color); // URP Unlit
            _material.SetColor("_UnlitColor", color); // HDRP Unlit
        }

        // Albedo
        XmlNode mainTexXml = channels.SelectSingleNode("_MainTex");
        if (mainTexXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(mainTexXml.InnerText);
            _material.SetTexture("_MainTex", texture);
            _material.SetTexture("_BaseMap", texture); // URP Unlit
            _material.SetTexture("_UnlitColorMap", texture); //HDRP Unlit
            _material.SetFloat("_UseColorMap", texture != null ? 1.0f : 0.0f);
        }

        // Metallic
        XmlNode metallicGlossMapXml = channels.SelectSingleNode("_MetallicGlossMap");
        if (metallicGlossMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(metallicGlossMapXml.InnerText);

            _material.EnableKeyword("_METALLICGLOSSMAP");
            if (texture)
            {
                _material.SetTexture("_MetallicGlossMap", texture);
                _material.SetFloat("_UseMetalicMap", 1.0f); // URP Autodesk
            }
            else
            {
                XmlNode metallicXml = channels.SelectSingleNode("_Metallic");
                if (metallicXml != null)
                {
                    float value = SmallImporterUtils.ParseFloatXml(metallicXml.InnerText);
                    _material.SetTexture("_MetallicGlossMap", null);
                    _material.SetFloat("_Metallic", value);
                    _material.SetFloat("_UseMetalicMap", 0.0f); // URP Autodesk
                }
            }
        }

        // Roughness
        XmlNode specMapXml = channels.SelectSingleNode("_SpecGlossMap");
        if (specMapXml != null)
        {
            _material.EnableKeyword("_SPECGLOSSMAP");
            Texture texture = SmallImporterUtils.ParseTextureXml(specMapXml.InnerText);
            if (texture)
            {
                _material.SetTexture("_SpecGlossMap", texture);
                _material.SetFloat("_UseRoughnessMap", 1.0f); // URP Autodesk
            }
            else
            {
                XmlNode glossinessXml = channels.SelectSingleNode("_Glossiness");
                if (glossinessXml != null)
                {
                    float value = SmallImporterUtils.ParseFloatXml(glossinessXml.InnerText);
                    _material.SetTexture("_SpecGlossMap", null);
                    _material.SetFloat("_Glossiness", value);
                    _material.SetFloat("_UseRoughnessMap", 0.0f); // URP Autodesk
                }
            }
        }

        // Normal Map
        XmlNode bumpMapXml = channels.SelectSingleNode("_BumpMap");
        if (bumpMapXml != null)
        {
            _material.EnableKeyword("_NORMALMAP");
            Texture texture = SmallImporterUtils.ParseTextureXml(bumpMapXml.InnerText);
            _material.SetTexture("_BumpMap", texture);
        }

        // Emission
        XmlNode emissionMapXml = channels.SelectSingleNode("_EmissionMap");
        if (emissionMapXml != null)
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(emissionMapXml.InnerText);
            if (texture)
            {
                _material.EnableKeyword("_EMISSION");
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                _material.SetTexture("_EmissionMap", texture);
                _material.SetColor("_EmissionColor", Color.white);
            }
        }
        else
        {
            Color color = Color.black;
            XmlNode emissionColorXml = channels.SelectSingleNode("_EmissionColor");
            if (emissionColorXml != null)
            {
                color = SmallImporterUtils.ParseColorXml(emissionColorXml.InnerText);
            }
            if (color != Color.black)
            {
                _material.EnableKeyword("_EMISSION");
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                _material.SetColor("_EmissionColor", color);
                _material.SetTexture("_EmissionMap", null);
            }
            else
            {
                _material.DisableKeyword("_EMISSION");
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        // Tile and lightmap data
        XmlNode tileMaxXml = channels.SelectSingleNode("_TileMax");
        if (tileMaxXml != null)
        {
            float tileMax = SmallImporterUtils.ParseFloatXml(tileMaxXml.InnerText);
            float tileScale = 1.0f / tileMax;
            _material.SetFloat("_TileScale", tileScale);

            float tileX = SmallImporterUtils.ParseFloatXml(channels.SelectSingleNode("_TileX").InnerText);
            float tileY = SmallImporterUtils.ParseFloatXml(channels.SelectSingleNode("_TileY").InnerText);
            _material.SetVector("_Offset", new Vector4(tileX * tileScale, tileY * tileScale));
            
            // Lightmap
            XmlNode lightmapXml = channels.SelectSingleNode("_Lightmap");
            if (lightmapXml != null)
            {
                Texture texture = SmallImporterUtils.ParseTextureXml(lightmapXml.InnerText);
                _material.SetTexture("_Lightmap", texture);
            }
        }

        // Transparent
        XmlNode transparentXml = channels.SelectSingleNode("_Transparent");
        if (transparentXml != null)
        {
            string value = transparentXml.InnerText;
            float mode = 0.0f;
            float threshold = 0.5f;
            if (value == "OPAQUE")
            {
                mode = 0.0f;
            }
            else if (value == "CLIP")
            {
                mode = 1.0f;
                XmlNode clipThresholdXml = channels.SelectSingleNode("_ClipThreshold");
                if (clipThresholdXml != null)
                {
                    threshold = SmallImporterUtils.ParseFloatXml(clipThresholdXml.InnerText);
                    Debug.Log("threshold" + clipThresholdXml.InnerText);
                }
            }
            else if (value == "BLEND")
            {
                mode = 2.0f;
            }
            // _Mode is for standard shaders
            _material.SetFloat("_Mode", mode);

            // Surface is for URP
            // Set surface to 1 if transparent
            _material.SetFloat("_Surface", mode > 0.0f ? 1.0f : 0.0f);
            _material.SetFloat("_AlphaClip", mode == 1.0f ? 1.0f : 0.0f);
            _material.SetFloat("_Cutoff", threshold);
        }

        EditorUtility.SetDirty(_material);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

}