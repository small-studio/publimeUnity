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
            float value = SmallParserUtils.ParseFloatXml(input);
            material.SetFloat(propertyName, value);
        }
        else if (type == "Vector")
        {
            Vector3 vector = SmallParserUtils.ParseVectorXml(input);
            material.SetVector(propertyName, vector);
        }
        else if (type == "Color")
        {
            Color color = SmallParserUtils.ParseColorXml(input);
            material.SetColor(propertyName, color);
        }
        else if (type == "Texture")
        {
            Texture texture = SmallParserUtils.ParseTextureXml(input);
            if (texture)
            {
                material.SetTexture(propertyName, texture);
            }
        }
    }
}

}