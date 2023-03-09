using UnityEngine;
using UnityEditor;
using System;

namespace SUBlime
{

public class SmallImporterWindow : EditorWindow
{
    public static string prefixPrefab = "_";
    public static SmallLogger.LogType logMask;
    public static ModelImporterMaterialImportMode materialImportMode = ModelImporterMaterialImportMode.None;

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
        materialImportMode = Enum.Parse<ModelImporterMaterialImportMode>(EditorPrefs.GetString("SBI_materialImportMode", prefixPrefab), true);
        logMask = (SmallLogger.LogType)EditorPrefs.GetInt("SBI_log", (int)logMask);
    }

    void OnGUI()
    {
        GUILayout.Label("Small Importer:", EditorStyles.boldLabel);
        prefixPrefab = EditorGUILayout.TextField("Prefix Identification", prefixPrefab);
        materialImportMode = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup("Material import mode", materialImportMode);
        logMask = (SmallLogger.LogType)EditorGUILayout.MaskField("Log", (int)logMask, Enum.GetNames(typeof(SmallLogger.LogType)));

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

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("SBI_prefixPrefab", prefixPrefab);
        EditorPrefs.SetString("SBI_materialImportMode", materialImportMode.ToString());
        EditorPrefs.SetInt("SBI_log", (int)logMask);
    }
}

}