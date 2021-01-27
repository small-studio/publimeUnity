using System;
using System.Collections.Generic;
using UnityEditor;

namespace SUBlime
{

public abstract class AAssetImporter
{
    struct AssetDependency
    {
        public string _assetPath;
        public Type _assetType;

        public AssetDependency(string assetPath, Type type)
        {
            _assetPath = assetPath;
            _assetType = type;
        }

        public bool CanLoad()
        {
            return AssetDatabase.LoadAssetAtPath(_assetPath, _assetType) != null;
        }
    }

    List<AssetDependency> _dependencies = new List<AssetDependency>();

    public void AddDependency<T>(string assetPath)
    {
        _dependencies.Add(new AssetDependency(assetPath, typeof(T)));
    }

    public bool CanLoadDependencies()
    {
        foreach (AssetDependency dependency in _dependencies)
        {
            if (!dependency.CanLoad())
            {
                return false;
            }
        }
        return true; 
    }

    public abstract void CreateDependencies(string assetPath);
    public abstract void OnPreImport(string assetPath);
    public abstract void OnPostImport(string assetPath);
}

}