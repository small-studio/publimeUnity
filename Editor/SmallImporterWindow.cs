using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.PackageManager;
using System.Collections;

namespace SUBlime
{

public class SmallImporterWindow : EditorWindow
{
    public static string prefixPrefab = "_";
    public static SmallLogger.LogType logMask;
    public static ModelImporterMaterialImportMode materialImportMode = ModelImporterMaterialImportMode.None;
    public static Shader defaultShader = null;
    public static string version = "1.9.2";

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
        LoadOptions();
    }

    static void LoadOptions()
    {
        materialImportMode = Enum.Parse<ModelImporterMaterialImportMode>(EditorPrefs.GetString("SBI_materialImportMode", prefixPrefab), true);
        logMask = (SmallLogger.LogType)EditorPrefs.GetInt("SBI_log", (int)logMask);
        defaultShader = Shader.Find(EditorPrefs.GetString("SBI_defaultShader", ""));
    }

    void OnGUI()
    {
        GUILayout.Label("Small Importer:", EditorStyles.boldLabel);
        materialImportMode = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup("Material import mode", materialImportMode);
        logMask = (SmallLogger.LogType)EditorGUILayout.MaskField("Log", (int)logMask, Enum.GetNames(typeof(SmallLogger.LogType)));
        defaultShader = (Shader)EditorGUILayout.ObjectField("Default shader", defaultShader, typeof(Shader), false);

        if (GUILayout.Button("Refresh SUBlime"))
        {
            SmallLogger.Log(SmallLogger.LogType.Debug, "Sublime refreshed.");
            SmallAssetPostprocessor.Reset();
        }

        GUILayout.BeginHorizontal();
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        GUILayout.Label("Status: ", style, GUILayout.ExpandWidth(false));
        if (SmallAssetPostprocessor.hasMissingDependencies)
        {
            style.normal.textColor = Color.red;
            GUILayout.Label("Missing dependencies", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUILayout.Label("All good", style);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        style.normal.textColor = Color.gray;
        GUILayout.Label("Version: " + version, style);
        EditorGUILayout.EndHorizontal();

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("SBI_materialImportMode", materialImportMode.ToString());
        EditorPrefs.SetInt("SBI_log", (int)logMask);
        EditorPrefs.SetString("SBI_defaultShader", defaultShader != null ? defaultShader.name : "");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void CreateAssetWhenReady()
    {
        SmallImporterWindow.LoadOptions();
    }
}

}