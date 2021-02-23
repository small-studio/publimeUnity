using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using UnityEngine.Events;

namespace SUBlime
{

public class MaterialImporter : AAssetImporter
{
    Material _material = null;
    EmissionMode _emissionMode = EmissionMode.None;
    static Dictionary<string, UnityAction<MaterialImporter, XmlNode, string>> _specialValues = null;

    public enum EmissionMode
    {
        None,
        Color,
        Texture,
        Both
    }

    public MaterialImporter()
    {
        if (_specialValues == null)
        {
            InitSpecialValues();
        }
    }

    static void InitSpecialValues()
    {
        _specialValues = new Dictionary<string, UnityAction<MaterialImporter, XmlNode, string>>();
        _specialValues["_MainTex"] = (importer, channels, propName) => { importer.UseColorMap(importer.SetTexture(channels, propName)); };
        _specialValues["_BaseMap"] = (importer, channels, propName) => { importer.UseColorMap(importer.SetTexture(channels, propName)); };
        _specialValues["_UnlitColorMap"] = (importer, channels, propName) => { importer.UseColorMap(importer.SetTexture(channels, propName)); };
        _specialValues["_MetallicGlossMap"] = (importer, channels, propName) => { importer.UseMetallicMap(importer.SetTexture(channels, propName)); };
        _specialValues["_ParallaxMap"] = (importer, channels, propName) => { importer.SetKeyword("_PARALLAXMAP", importer.SetTexture(channels, propName)); };
        _specialValues["_BumpMap"] = (importer, channels, propName) => { importer.SetKeyword("_NORMALMAP", importer.SetTexture(channels, propName)); };
        _specialValues["_EmissionMap"] = (importer, channels, propName) => { importer.UseEmission(importer.SetTexture(channels, propName), EmissionMode.Texture); };
        _specialValues["_EmissionColor"] = (importer, channels, propName) => { importer.UseEmission(importer.SetColor(channels, propName), EmissionMode.Color); };
        _specialValues["_Transparent"] = (importer, channels, propName) => { importer.SetTransparency(channels, propName); };
    }

    public override void CreateDependencies(string assetPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(assetPath);
        XmlNode root = xml.DocumentElement;

        // Add textures dependencies
        XmlNode channels = root.SelectSingleNode("Channels");
        if (channels != null)
        {
            foreach (XmlNode node in channels.ChildNodes)
            {
                if (node != null && SmallParserUtils.ParseTextureXml(node.InnerText))
                {
                    AddDependency<Texture>(SmallImporterUtils.GetTexturePath(node.InnerText));
                }
            }
        }
    }

    public override void OnPreImport(string assetPath)
    {
        _material = SmallImporterUtils.CreateMaterialFromXml(assetPath);
    }

    public bool SetTexture(XmlNode channels, string textureName)
    {
        XmlNode nodeXml = channels.SelectSingleNode(textureName);
        if (nodeXml != null)
        {
            Texture texture = SmallParserUtils.ParseTextureXml(nodeXml.InnerText);
            _material.SetTexture(textureName, texture);
            return texture != null;
        }
        return false;
    }

    public bool SetColor(XmlNode channels, string valueName)
    {
        XmlNode nodeXml = channels.SelectSingleNode(valueName);
        if (nodeXml != null)
        {
            Color color = SmallParserUtils.ParseColorXml(nodeXml.InnerText);
            _material.SetColor(valueName, color);
            return color != Color.black;
        }
        return false;
    }

    public bool SetVector(XmlNode channels, string valueName)
    {
        XmlNode nodeXml = channels.SelectSingleNode(valueName);
        if (nodeXml != null)
        {
            Vector3 vector = SmallParserUtils.ParseVectorXml(nodeXml.InnerText);
            _material.SetVector(valueName, vector);
            return true;
        }
        return false;
    }

    public bool SetFloat(XmlNode channels, string valueName)
    {
        XmlNode nodeXml = channels.SelectSingleNode(valueName);
        if (nodeXml != null)
        {
            float value = SmallParserUtils.ParseFloatXml(nodeXml.InnerText);
            _material.SetFloat(valueName, value);
            return true;
        }
        return false;
    }

    public void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            _material.EnableKeyword(keyword);
        }
        else
        {
            _material.DisableKeyword(keyword);
        }
    }

    public void UseColorMap(bool enable)
    {
        _material.SetFloat("_UseColorMap", enable ? 1.0f : 0.0f); // URP Autodesk
    }

    public void UseMetallicMap(bool enable)
    {
        SetKeyword("_METALLICGLOSSMAP", enable);
        _material.SetFloat("_UseMetalicMap", enable ? 1.0f : 0.0f); // URP Autodesk
    }

    public void UseEmission(bool enable, EmissionMode mode)
    {
        if (_emissionMode == EmissionMode.None)
        {
            SetKeyword("_EMISSION", enable);
            if (enable)
            {
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                _emissionMode = mode;
            }
            else
            {
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
        else
        {
            _emissionMode = EmissionMode.Both;
        }
    }

    public void SetTransparency(XmlNode channels, string propName)
    {
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
                XmlNode clipThresholdXml = channels.SelectSingleNode("_Cutoff");
                if (clipThresholdXml != null)
                {
                    threshold = SmallParserUtils.ParseFloatXml(clipThresholdXml.InnerText);
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
    }

    public void SetLightmap(XmlNode channels, string propName)
    {
        // Tile and lightmap data
        XmlNode tileMaxXml = channels.SelectSingleNode("_TileMax");
        if (tileMaxXml != null)
        {
            float tileMax = SmallParserUtils.ParseFloatXml(tileMaxXml.InnerText);
            float tileScale = 1.0f / tileMax;
            _material.SetFloat("_TileScale", tileScale);

            float tileX = SmallParserUtils.ParseFloatXml(channels.SelectSingleNode("_TileX").InnerText);
            float tileY = SmallParserUtils.ParseFloatXml(channels.SelectSingleNode("_TileY").InnerText);
            _material.SetVector("_Offset", new Vector4(tileX * tileScale, tileY * tileScale));
            
            // Lightmap
            XmlNode lightmapXml = channels.SelectSingleNode("_Lightmap");
            if (lightmapXml != null)
            {
                Texture texture = SmallParserUtils.ParseTextureXml(lightmapXml.InnerText);
                _material.SetTexture("_Lightmap", texture);
            }
        }
    }

    public void SetMaterialValue(XmlNode channels, string propertyName, string input)
    {
        string type = input.Split(',')[0];
        if (type == "Float")
        {
            SetFloat(channels, propertyName);
        }
        else if (type == "Vector")
        {
            SetVector(channels, propertyName);
        }
        else if (type == "Color")
        {
            SetColor(channels, propertyName);
        }
        else if (type == "Texture")
        {
            SetTexture(channels, propertyName);
        }
    }

    public override void OnPostImport(string assetPath)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(assetPath);
        XmlNode root = xml.DocumentElement;

        // Get Data from Channels when available
        XmlNode channels = root.SelectSingleNode("Channels");
        if (channels != null)
        {
            foreach (XmlNode node in channels.ChildNodes)
            {
                if (_specialValues.ContainsKey(node.Name))
                {
                    Debug.Log("Special value" + node.Name);
                    _specialValues[node.Name].Invoke(this, channels, node.Name);
                }
                else
                {
                    SetMaterialValue(channels, node.Name, node.InnerText);
                }
            }
        }

        EditorUtility.SetDirty(_material);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

}