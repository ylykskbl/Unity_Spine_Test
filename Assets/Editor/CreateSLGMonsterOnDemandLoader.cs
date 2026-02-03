using UnityEngine;
using UnityEditor;
using Spine.Unity;

/// <summary>
/// Menu item to create SLGMonsterOnDemandLoader asset and assign it to SLGMonster(动画)_Atlas.
/// Run once via menu: Spine > Create SLG Monster On Demand Loader.
/// </summary>
public static class CreateSLGMonsterOnDemandLoader
{
    private const string LoaderAssetPath = "Assets/Resources/SpineAnimations/SLGMonsterOnDemandLoader.asset";
    private const string AtlasPath = "Assets/Resources/SpineAnimations/SLGMonster(动画)_Atlas.asset";

    [MenuItem("Spine/Create SLG Monster On Demand Loader")]
    public static void CreateAndWire()
    {
        var atlas = AssetDatabase.LoadAssetAtPath<AtlasAssetBase>(AtlasPath);
        if (atlas == null)
        {
            Debug.LogError("Atlas not found at: " + AtlasPath);
            return;
        }

        var existingLoader = AssetDatabase.LoadAssetAtPath<SLGMonsterOnDemandLoader>(LoaderAssetPath);
        SLGMonsterOnDemandLoader loader;
        if (existingLoader != null)
        {
            loader = existingLoader;
            Debug.Log("Using existing loader at: " + LoaderAssetPath);
        }
        else
        {
            loader = ScriptableObject.CreateInstance<SLGMonsterOnDemandLoader>();
            AssetDatabase.CreateAsset(loader, LoaderAssetPath);
            Debug.Log("Created loader at: " + LoaderAssetPath);
        }

        loader.atlasAsset = atlas;
        EditorUtility.SetDirty(loader);
        AssetDatabase.SaveAssets();

        var so = new SerializedObject(atlas);
        so.FindProperty("textureLoadingMode").intValue = 1; // OnDemand
        so.FindProperty("onDemandTextureLoader").objectReferenceValue = loader;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(atlas);
        AssetDatabase.SaveAssets();

        Debug.Log("SLG Monster On Demand Loader created and assigned to atlas. Set Atlas's 'Texture Loading Mode' to On Demand if not already.");
    }
}
