using UnityEngine;
using UnityEditor;
using System;

namespace SUBlime
{

public class SmallImporterWindow : EditorWindow
{
    public static string prefixPrefab = "_";
    public static SmallLogger.LogType logMask;

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
        logMask = (SmallLogger.LogType)EditorPrefs.GetInt("SBI_log", (int)logMask);
    }

    void OnGUI()
    {
        GUILayout.Label("Small Importer:", EditorStyles.boldLabel);
        prefixPrefab = EditorGUILayout.TextField("Prefix Identification", prefixPrefab);
        logMask = (SmallLogger.LogType)EditorGUILayout.MaskField("Log", (int)logMask, Enum.GetNames(typeof(SmallLogger.LogType)));

        if (GUILayout.Button("Refresh SUBlime"))
        {
            SmallLogger.Log(SmallLogger.LogType.Debug, "Sublime refreshed.");
            SmallAssetPostprocessor.Reset();
        }

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("SBI_prefixPrefab", prefixPrefab);
        EditorPrefs.SetInt("SBI_log", (int)logMask);
    }
}

}