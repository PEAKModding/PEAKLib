using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using PEAKLib.Core.UnityEditor;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PEAKLib.Core;

/// <summary>
/// Loads AssetBundles asynchronously.
/// </summary>
public static class BundleLoader
{
    /// <summary>
    /// An event that runs when all AssetBundles are loaded by PEAKLib.
    /// </summary>
    public static event Action? OnAllBundlesLoaded;

    private static readonly List<LoadOperation> _operations = [];

    internal static void LoadAllBundles(string root, string withExtension)
    {
        CorePlugin.Log.LogInfo(
            $"Loading all bundles with extension {withExtension} from root {root}"
        );

        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            LoadBundleAndContents(path);
        }
    }

    /// <param name="baseUnityPlugin">An instance of a <see cref="BaseUnityPlugin"/> type
    /// whose <see cref="ModDefinition"/> to use.</param>
    /// <param name="fileName">The full file name without path. This is searched for recursively
    /// from the directory of the <see cref="BaseUnityPlugin"/> assembly.</param>
    /// <inheritdoc cref="LoadBundleFromPath(string, Action{AssetBundle, ModDefinition})"/>
    /// <param name="onLoaded"></param>
    public static void LoadBundleWithName(
        this BaseUnityPlugin baseUnityPlugin,
        string fileName,
        Action<AssetBundle, ModDefinition> onLoaded
    )
    {
        ThrowHelper.ThrowIfArgumentNull(baseUnityPlugin);
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(fileName);
        ThrowHelper.ThrowIfArgumentNull(onLoaded);

        var root = Path.GetDirectoryName(baseUnityPlugin.Info.Location);
        string[] files = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);

        var modDefinition = ModDefinition.GetOrCreate(baseUnityPlugin.Info);

        foreach (string path in files)
        {
            CorePlugin.Log.LogInfo($"Loading bundle at {path}...");
            _operations.Add(new LoadOperation(path, onLoaded, loadContents: false, modDefinition));
        }
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Does not register its contents automatically.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    /// <param name="onLoaded">Callback for when the AssetBundle is loaded.</param>
    public static void LoadBundleFromPath(string path, Action<AssetBundle, ModDefinition> onLoaded)
    {
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(path);
        ThrowHelper.ThrowIfArgumentNull(onLoaded);

        CorePlugin.Log.LogInfo($"Loading bundle at {path}...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents: false));
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Also registers its contents automatically.
    /// </summary>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    /// <param name="onLoaded">Callback for when the AssetBundle is loaded. Supports waiting as an IEnumerator coroutine.</param>
    public static void LoadBundleAndContents(
        string path,
        Action<AssetBundle, ModDefinition>? onLoaded = null
    )
    {
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(path);

        CorePlugin.Log.LogInfo($"Loading bundle at {path}...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents: true));
    }

    internal static void FinishLoadOperations(MonoBehaviour behaviour)
    {
        behaviour.StartCoroutine(FinishLoadOperationsRoutine(behaviour));
    }

    private static IEnumerator FinishLoadOperationsRoutine(MonoBehaviour behaviour)
    {
        yield return null;

        foreach (var loadOperation in _operations.ToArray()) // collection might change
        {
            behaviour.StartCoroutine(FinishLoadOperation(loadOperation));
        }

        var (text, disableLoadingUI) = SetupLoadingUI();

        float lastUpdate = Time.time;
        while (_operations.Count > 0)
        {
            if (Time.time - lastUpdate > 1)
            {
                lastUpdate = Time.time;

                string bundlesWord = _operations.Count == 1 ? "bundle" : "bundles";
                // text.text = $"PEAKLib: Waiting for {_operations.Count} {bundlesWord} to load...";

                // if (!ConfigManager.ExtendedLogging.Value)
                //     continue;

                foreach (var operation in _operations)
                {
                    string msg = $"Loading {operation.FileName}: {operation.CurrentState}";
                    float? progress = operation.CurrentState switch
                    {
                        LoadOperation.State.LoadingBundle => operation.BundleRequest.progress,
                        _ => null,
                    };

                    if (progress.HasValue)
                        msg += $" {progress.Value:P0}";

                    CorePlugin.Log.LogDebug(msg);
                }
            }

            yield return null;
        }

        CorePlugin.Log.LogInfo("Finished loading bundles.");

        disableLoadingUI();

        if (OnAllBundlesLoaded is null)
        {
            yield break;
        }

        foreach (var callback in OnAllBundlesLoaded.GetInvocationList())
        {
            try
            {
                ((Action)callback)();
            }
            catch (Exception ex)
            {
                CorePlugin.Log.LogError($"Unhandled exception in callback: {ex}");
            }
        }
    }

    private static (TMP_Text, Action) SetupLoadingUI()
    {
        // TODO: UI

        // var hudCanvas = GameObject.Find("HUD Canvas");
        // var hud = hudCanvas.transform.Find("HUD");
        // hud.gameObject.SetActive(false);

        // var buttonText = Object.FindObjectOfType<TMP_Text>();
        // var text = Object.Instantiate(buttonText, hudCanvas.transform);
        // text.gameObject.name = "PEAKLibText";
        // text.gameObject.SetActive(true);
        // text.text = "PEAKLib is loading bundles... Hang tight!";
        // text.color = Color.white;
        // text.alignment = TextAlignmentOptions.Center;

        // var rectTransform = text.GetComponent<RectTransform>();
        // rectTransform.anchoredPosition = Vector2.zero;
        // rectTransform.anchorMin = Vector2.zero;
        // rectTransform.anchorMax = Vector2.one;
        // rectTransform.sizeDelta = Vector2.zero;

        return (
            null!, // text,
            () => {
                // text.gameObject.SetActive(false);
                // hud.gameObject.SetActive(true);
            }
        );
    }

    private static IEnumerator FinishLoadOperation(LoadOperation operation)
    {
        yield return operation.BundleRequest;
        var bundle = operation.BundleRequest.assetBundle;

        if (bundle == null)
        {
            CorePlugin.Log.LogError($"Failed to load bundle {operation.FileName}!");
            Finish();
            yield break;
        }

        operation.CurrentState = LoadOperation.State.LoadingContent;
        var assetRequest = bundle.LoadAllAssetsAsync<ScriptableObject>();
        yield return assetRequest;

        Object[] assets = assetRequest.allAssets;

        var mods = assets.OfType<UnityModDefinition>().Select(x => x.Resolve()).ToList();
        if (operation.ModDefinition is { } modDefinition)
        {
            mods.Add(modDefinition);
        }

        switch (mods.Count)
        {
            case 0:
                CorePlugin.Log.LogError($"Bundle {operation.FileName} contains no mods!");
                Finish();
                yield break;
            case > 1:
                CorePlugin.Log.LogError($"Bundle {operation.FileName} contains more than one mod!");
                Finish();
                yield break;
            default:
                break;
        }

        var mod = mods[0];

        if (operation.LoadContents)
        {
            foreach (var content in assets.OfType<IContent>())
            {
                try
                {
                    mod.Register(content);
                }
                catch (Exception e)
                {
                    CorePlugin.Log.LogError(
                        $"Failed to register {mod.Id}:{content} ({content.GetType().Name}) from bundle {operation.FileName}: {e}"
                    );
                }
            }
        }
        else
        {
            foreach (var content in assets.OfType<IContent>())
            {
                mod.Content.Add(content);
            }
        }

        operation.OnBundleLoaded?.Invoke(bundle, mod);

        // if (ConfigManager.ExtendedLogging.Value)
        // {
        CorePlugin.Log.LogInfo(
            $"Loaded bundle {operation.FileName} in {operation.ElapsedTime.TotalSeconds:N1}s"
        );
        // }

        Finish();
        yield break;

        void Finish()
        {
            _operations.Remove(operation);
        }
    }

    private class LoadOperation(
        string path,
        Action<AssetBundle, ModDefinition>? onBundleLoaded = null,
        bool loadContents = true,
        ModDefinition? modDefinition = null
    )
    {
        public string Path { get; } = path;
        public DateTime StartTime { get; } = DateTime.Now;
        public State CurrentState { get; set; } = State.LoadingBundle;
        public bool LoadContents { get; } = loadContents;
        public ModDefinition? ModDefinition { get; } = modDefinition;
        public Action<AssetBundle, ModDefinition>? OnBundleLoaded { get; } = onBundleLoaded;

        public AssetBundleCreateRequest BundleRequest { get; } =
            AssetBundle.LoadFromFileAsync(path);

        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

        public enum State
        {
            LoadingBundle,
            LoadingContent,
        }
    }
}
