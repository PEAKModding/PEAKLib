using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// A PEAKLib wrapper for an <see cref="AssetBundle"/>.
/// </summary>
public sealed class PeakBundle
{
    /// <summary>
    /// The <see cref="ModDefinition"/> that owns this <see cref="PeakBundle"/>.
    /// </summary>
    public ModDefinition Mod { get; }

    private readonly AssetBundle bundle;

    internal PeakBundle(AssetBundle assetBundle, ModDefinition modDefinition)
    {
        bundle = ThrowHelper.ThrowIfArgumentNull(assetBundle);
        Mod = ThrowHelper.ThrowIfArgumentNull(modDefinition);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Object LoadAsset(string name) => bundle.LoadAsset(name);

    public T LoadAsset<T>(string name)
        where T : Object => bundle.LoadAsset<T>(name);

    public AssetBundleRequest LoadAssetAsync(string name) => bundle.LoadAssetAsync(name);

    public AssetBundleRequest LoadAssetAsync<T>(string name) => bundle.LoadAssetAsync<T>(name);

    public Object[] LoadAssetWithSubAssets(string name) => bundle.LoadAssetWithSubAssets(name);

    public T[] LoadAssetWithSubAssets<T>(string name)
        where T : Object => bundle.LoadAssetWithSubAssets<T>(name);

    public AssetBundleRequest LoadAssetWithSubAssetsAsync(string name) =>
        bundle.LoadAssetWithSubAssetsAsync(name);

    public AssetBundleRequest LoadAssetWithSubAssetsAsync<T>(string name) =>
        bundle.LoadAssetWithSubAssetsAsync<T>(name);

    public Object[] LoadAllAssets() => bundle.LoadAllAssets();

    public T[] LoadAllAssets<T>()
        where T : Object => bundle.LoadAllAssets<T>();

    public AssetBundleRequest LoadAllAssetsAsync() => bundle.LoadAllAssetsAsync();

    public AssetBundleRequest LoadAllAssetsAsync<T>() => bundle.LoadAllAssetsAsync<T>();

    public string[] GetAllAssetNames() => bundle.GetAllAssetNames();

    public string[] GetAllScenePaths() => bundle.GetAllScenePaths();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
