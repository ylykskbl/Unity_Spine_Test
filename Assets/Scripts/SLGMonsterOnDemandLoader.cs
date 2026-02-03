// On-Demand Texture Loader for Spine Atlas: loads only the texture page for the currently displayed skin.
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(fileName = "SLGMonsterOnDemandLoader", menuName = "Spine/On Demand Texture Loader (SLG Monster)")]
public class SLGMonsterOnDemandLoader : OnDemandTextureLoader
{
    public const string PlaceholderTextureName = "SpineOnDemandPlaceholder";
    public const string ResourcesPathPrefix = "SpineAnimations/";

    private Texture2D _placeholderTexture;
    private List<string> _pageNames;
    private readonly Dictionary<Material, Texture> _loadedTextures = new Dictionary<Material, Texture>();

    private Texture2D GetOrCreatePlaceholder()
    {
        if (_placeholderTexture != null) return _placeholderTexture;
        _placeholderTexture = new Texture2D(1, 1);
        _placeholderTexture.name = PlaceholderTextureName;
        _placeholderTexture.SetPixel(0, 0, Color.white);
        _placeholderTexture.Apply();
        return _placeholderTexture;
    }

    private List<string> GetPageNames()
    {
        if (_pageNames != null && _pageNames.Count > 0) return _pageNames;
        _pageNames = new List<string>();
        if (atlasAsset == null) return _pageNames;
        var spineAtlas = atlasAsset as SpineAtlasAsset;
        if (spineAtlas == null || spineAtlas.atlasFile == null) return _pageNames;
        string text = spineAtlas.atlasFile.text;
        if (string.IsNullOrEmpty(text)) return _pageNames;
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.EndsWith(".png"))
                _pageNames.Add(line.Replace(".png", ""));
        }
        return _pageNames;
    }

    private int GetMaterialIndex(Material material)
    {
        if (atlasAsset == null) return -1;
        int i = 0;
        foreach (Material m in atlasAsset.Materials)
        {
            if (m == material) return i;
            i++;
        }
        return -1;
    }

    public override string GetPlaceholderTextureName(string originalTextureName)
    {
        return PlaceholderTextureName;
    }

    public override bool AssignPlaceholderTextures(out IEnumerable<Material> modifiedMaterials)
    {
        var list = new List<Material>();
        modifiedMaterials = list;
        if (atlasAsset == null) return false;
        Texture2D placeholder = GetOrCreatePlaceholder();
        foreach (Material m in atlasAsset.Materials)
        {
            if (m == null) continue;
            m.mainTexture = placeholder;
            list.Add(m);
        }
        return list.Count > 0;
    }

    public override bool HasPlaceholderTexturesAssigned(out List<Material> placeholderMaterials)
    {
        placeholderMaterials = new List<Material>();
        if (atlasAsset == null) return false;
        foreach (Material m in atlasAsset.Materials)
        {
            if (m != null && m.mainTexture != null && m.mainTexture.name == PlaceholderTextureName)
                placeholderMaterials.Add(m);
        }
        return placeholderMaterials.Count > 0;
    }

    public override bool AssignTargetTextures(out IEnumerable<Material> modifiedMaterials)
    {
        var list = new List<Material>();
        modifiedMaterials = list;
        if (atlasAsset == null) return false;
        var pageNames = GetPageNames();
        int index = 0;
        foreach (Material m in atlasAsset.Materials)
        {
            if (m == null) continue;
            if (index < pageNames.Count)
            {
                var tex = Resources.Load<Texture2D>(ResourcesPathPrefix + pageNames[index]);
                if (tex != null)
                {
                    m.mainTexture = tex;
                    list.Add(m);
                }
            }
            index++;
        }
        return list.Count > 0;
    }

    public override void BeginCustomTextureLoading() { }

    public override void EndCustomTextureLoading() { }

    public override bool HasPlaceholderAssigned(Material material)
    {
        return material != null && material.mainTexture != null && material.mainTexture.name == PlaceholderTextureName;
    }

    public override void RequestLoadMaterialTextures(Material material, ref Material overrideMaterial)
    {
        if (atlasAsset == null || material == null) return;
        if (!HasPlaceholderAssigned(material)) return;

        int index = GetMaterialIndex(material);
        var pageNames = GetPageNames();
        if (index < 0 || index >= pageNames.Count) return;

        string pageName = pageNames[index];
        var tex = Resources.Load<Texture2D>(ResourcesPathPrefix + pageName);
        if (tex == null) return;

        overrideMaterial = new Material(material);
        overrideMaterial.mainTexture = tex;
        _loadedTextures[overrideMaterial] = tex;
        Debug.Log("[SLGMonsterOnDemandLoader] Loaded texture page: " + pageName);
    }

    public override void RequestLoadTexture(Texture placeholderTexture, ref Texture replacementTexture, System.Action<Texture> onTextureLoaded = null)
    {
        if (atlasAsset == null || placeholderTexture == null) return;
        if (placeholderTexture.name != PlaceholderTextureName) return;

        int foundIndex = -1;
        int i = 0;
        foreach (Material m in atlasAsset.Materials)
        {
            if (m != null && m.mainTexture == placeholderTexture) { foundIndex = i; break; }
            i++;
        }
        if (foundIndex < 0) return;

        var pageNames = GetPageNames();
        if (foundIndex >= pageNames.Count) return;

        var tex = Resources.Load<Texture2D>(ResourcesPathPrefix + pageNames[foundIndex]);
        if (tex != null)
        {
            replacementTexture = tex;
            onTextureLoaded?.Invoke(tex);
        }
    }

    public override void Clear(bool clearAtlasAsset = false)
    {
        _loadedTextures.Clear();
        if (clearAtlasAsset) atlasAsset = null;
        _pageNames = null;
    }
}
