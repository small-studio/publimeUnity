using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SUBlime
{

class SmallAssetPostprocessor : AssetPostprocessor
{
    static Dictionary<string, AAssetImporter> _importers = new Dictionary<string, AAssetImporter>();
    static public bool hasMissingDependencies => _importers.Count != 0;

    public static void Reset()
    {
        _importers.Clear();
    }

    AAssetImporter GetAssetImporter(string path)
    {
        if (path.EndsWith(SmallImporterUtils.SMALL_MATERIAL_EXTENSION))
        {
            return new MaterialImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_PREFAB_EXTENSION))
        {
            return new PrefabImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_SCENE_EXTENSION))
        {
            return new SceneImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_LIGHT_EXTENSION))
        {
            return new LightImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_CAMERA_EXTENSION))
        {
            return new CameraImporter();
        }
        return null;
    }

#region PreProcess
    void OnPreprocessAsset()
    {
        // During this phase we create the needed assets without linking them together
        AAssetImporter importer = GetAssetImporter(assetPath);
        if (importer != null)
        {
            if (!_importers.ContainsKey(assetPath))
            {
                importer.CreateDependencies(assetPath);
                SmallLogger.Log(SmallLogger.LogType.AssetPostProcessor, "Importing asset: " + Path.GetFileName(assetPath) + " with " + importer.DependencyCount + (importer.DependencyCount == 1 ? " dependency" : " dependencies"));
                importer.OnPreImport(assetPath);
                _importers.Add(assetPath, importer);
            }
            else
            {
                SmallLogger.LogError(SmallLogger.LogType.AssetPostProcessor, "Asset already in the list : " + assetPath);
            }
        }
    }

    void OnPreprocessModel()
    {
        if (Path.GetFileName(assetPath).StartsWith(SmallImporterWindow.prefixPrefab))
        {
            // Don't import materials for models
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.materialImportMode = SmallImporterWindow.materialImportMode;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        }
    }

    void OnPreprocessTexture()
    {
        if (assetPath.Contains("_nrm"))
        {
            TextureImporter textureImporter = assetImporter as TextureImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
        }
        else if (assetPath.Contains("_alpha"))
        {
            TextureImporter textureImporter = assetImporter as TextureImporter;
            textureImporter.alphaIsTransparency = true;
        }
    }
#endregion

#region PostProcess
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // Post import all assets if dependecies can be loaded
        List<string> toDelete = new List<string>();
        foreach (KeyValuePair<string, AAssetImporter> item in _importers)
        {
            if (item.Value.CanLoadDependencies())
            {
                SmallLogger.Log(SmallLogger.LogType.AssetPostProcessor, "Post import asset: " + item.Key);
                item.Value.OnPostImport(item.Key);
                toDelete.Add(item.Key);
            }
        }

        // Delete imported assets
        foreach (string key in toDelete)
        {
            _importers.Remove(key);
        }

        SmallLogger.Log(SmallLogger.LogType.Dependency, _importers.Count + " asset(s) waiting for dependencies");
        foreach (KeyValuePair<string, AAssetImporter> item in _importers)
        {
            SmallLogger.Log(SmallLogger.LogType.Dependency, Path.GetFileName(item.Key) + " -> " + item.Value.ToString());
        }
    }
#endregion
}

}