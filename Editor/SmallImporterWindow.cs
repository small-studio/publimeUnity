using UnityEngine;
using UnityEditor;

namespace SUBlime
{

public class SmallImporterWindow : EditorWindow
{
    public static string prefixPrefab = "_";

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
    }

    void OnGUI()
    {
        GUILayout.Label("Small Importer:", EditorStyles.boldLabel);
        prefixPrefab = EditorGUILayout.TextField("Prefix Identification", prefixPrefab);

        if (GUILayout.Button("Refresh SUBlime"))
        {
            Debug.Log("Sublime refreshed.");
            SmallAssetPostprocessor.Reset();
        }

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("SBI_prefixPrefab", prefixPrefab);
    }
}

}