using UnityEngine;
using UnityEditor;

namespace SUBlime
{

public class SmallImporterWindow : EditorWindow
{
    public static string prefixPrefab = "_";
    public static Shader customShader = null;
    public static Shader customShaderTransparent = null;

    [MenuItem("Window/Small Importer")]
    static void Init()
    {
        SmallImporterWindow window = (SmallImporterWindow)EditorWindow.GetWindow(typeof(SmallImporterWindow));
        GUIContent title = new GUIContent();
        title.text = "Small Importer";
        window.titleContent = title;
        window.Show();
    }

    void Awake()
    {
        prefixPrefab = EditorPrefs.GetString("SBI_prefixPrefab", prefixPrefab);
        string shaderPath = EditorPrefs.GetString("SBI_customShader", customShader != null ? customShader.name : "");
        customShader = Shader.Find(shaderPath);
        shaderPath = EditorPrefs.GetString("SBI_customShaderTransparent", customShaderTransparent != null ? customShaderTransparent.name : "");
        customShaderTransparent = Shader.Find(shaderPath);
    }

    void OnGUI()
    {
        GUILayout.Label("Small Importer:", EditorStyles.boldLabel);
        prefixPrefab = EditorGUILayout.TextField("Prefix Identification", prefixPrefab);
        customShader = EditorGUILayout.ObjectField("Custom shader", customShader, typeof(Shader), true) as Shader;
        customShaderTransparent = EditorGUILayout.ObjectField("Custom shader transparent", customShaderTransparent, typeof(Shader), true) as Shader;

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("SBI_prefixPrefab", prefixPrefab);
        EditorPrefs.SetString("SBI_customShader", customShader != null ? customShader.name : "");
        EditorPrefs.SetString("SBI_customShaderTransparent", customShaderTransparent != null ? customShaderTransparent.name : "");
    }
}

}