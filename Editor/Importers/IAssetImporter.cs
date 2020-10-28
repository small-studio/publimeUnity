using UnityEditor;

namespace SUBlime
{

public interface IAssetImporter
{
    void OnPreImport(string assetPath, AssetImporter importer);
    void OnPostImport(string assetPath);
}

}