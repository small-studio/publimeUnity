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

        public bool IsEqual(AssetDependency dependency)
        {
            return _assetPath == dependency._assetPath;
        }

        public override string ToString()
        {
            return System.IO.Path.GetFileName(_assetPath) + " (" + _assetType.ToString() + ")";
        }
    }

    List<AssetDependency> _dependencies = new List<AssetDependency>();

    public void AddDependency<T>(string assetPath)
    {
        bool duplicate = false;
        AssetDependency newDependency = new AssetDependency(assetPath, typeof(T));
        foreach (AssetDependency dependency in _dependencies)
        {
            if (dependency.IsEqual(newDependency))
            {
                duplicate = true;
            }
        }

        if (!duplicate)
        {
            SmallLogger.Log(SmallLogger.LogType.Dependency, "Add dependency '" + newDependency.ToString() + "' for asset '" + System.IO.Path.GetFileName(assetPath) + "'"); 
            _dependencies.Add(newDependency);
        }
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

    public int DependencyCount
    {
        get { return _dependencies.Count; }
    }

    public override string ToString()
    {
        string s = "";
        foreach (AssetDependency dep in _dependencies)
        {
            s += dep.ToString();
        }
        return s;
    }

    public abstract void CreateDependencies(string assetPath);
    public abstract void OnPreImport(string assetPath);
    public abstract void OnPostImport(string assetPath);
}

}