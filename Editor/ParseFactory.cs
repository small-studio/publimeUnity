using UnityEngine;

namespace SUBlime
{

public class ParseFactory
{
    public static void Parse(string input, string propertyName, Material material)
    {
        string type = input.Split(',')[0];
        if (type == "Float")
        {
            float value = SmallImporterUtils.ParseFloatXml(input);
            material.SetFloat(propertyName, value);
        }
        else if (type == "Vector")
        {
            Vector3 vector = SmallImporterUtils.ParseVectorXml(input);
            material.SetVector(propertyName, vector);
        }
        else if (type == "Color")
        {
            Color color = SmallImporterUtils.ParseColorXml(input);
            material.SetColor(propertyName, color);
        }
        else if (type == "Texture")
        {
            Texture texture = SmallImporterUtils.ParseTextureXml(input);
            if (texture)
            {
                material.SetTexture(propertyName, texture);
            }
        }
    }
}

}