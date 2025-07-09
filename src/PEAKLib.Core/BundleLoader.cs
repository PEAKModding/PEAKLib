#if !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx;
using PEAKLib.Core.Extensions;
using PEAKLib.Core.UnityEditor;
using TMPro;
using UnityEngine;
using Zorro.Core;
using Object = UnityEngine.Object;

namespace PEAKLib.Core;

/// <summary>
/// Loads AssetBundles asynchronously.
/// </summary>
public static class BundleLoader
{
    /// <summary>
    /// An event that is fired when all AssetBundles are loaded by PEAKLib.
    /// This may happen during the main menu or earlier,
    /// but the deadline is on the airport loading screen.
    /// </summary>
    /// <remarks>
    /// It's possible an AssetBundle is loaded after this event,
    /// but it should be assumed that those bundles are reloaded in-game
    /// for development purposes to avoid having to reload the whole game again.
    /// You can support this scenario by by using the <see cref="OnBundleLoaded"/> event.
    /// </remarks>
    public static event Action? OnAllBundlesLoaded;

    /// <summary>
    /// And even that is fired for every AssetBundle loaded by PEAKLib.
    /// </summary>
    public static event Action<PeakBundle>? OnBundleLoaded;

    private static bool calledOnBundleLoaded;
    internal static bool bundleLoadingWindowClosed;

    private static readonly List<LoadOperation> _operations = [];

    private static void AddOperation(LoadOperation operation)
    {
        if (bundleLoadingWindowClosed)
        {
            CorePlugin.Log.LogWarning(
                "The bundle loading window has passed. "
                    + "Set your asset bundle to load in your plugin's Awake method "
                    + $"for the best chances of everything working properly.\n{new StackTrace()}"
            );
        }

        _operations.Add(operation);
    }

    internal static void LoadAllBundles(string root, string withExtension)
    {
        CorePlugin.Log.LogInfo(
            $"Loading all bundles with extension {withExtension} from root {root}"
        );

        string[] files = Directory.GetFiles(root, "*" + withExtension, SearchOption.AllDirectories);

        foreach (string path in files)
        {
            LoadBundleAndContentsFromPath(path);
        }
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Does not register its contents automatically.
    /// </summary>
    /// <inheritdoc cref="LoadBundleWithNameInternal(BaseUnityPlugin, string, Action{PeakBundle}, bool)"/>
    public static void LoadBundleWithName(
        this BaseUnityPlugin baseUnityPlugin,
        string fileName,
        Action<PeakBundle> onLoaded
    ) => LoadBundleWithNameInternal(baseUnityPlugin, fileName, onLoaded, loadContents: false);

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Also registers its contents automatically.
    /// </summary>
    /// <inheritdoc cref="LoadBundleWithNameInternal(BaseUnityPlugin, string, Action{PeakBundle}, bool)"/>
    public static void LoadBundleAndContentsWithName(
        this BaseUnityPlugin baseUnityPlugin,
        string fileName,
        Action<PeakBundle> onLoaded
    ) => LoadBundleWithNameInternal(baseUnityPlugin, fileName, onLoaded, loadContents: true);

    /// <summary></summary>
    /// <remarks></remarks>
    /// <param name="baseUnityPlugin">An instance of a <see cref="BaseUnityPlugin"/> type
    /// whose <see cref="ModDefinition"/> to use.</param>
    /// <param name="fileName">The full file name without path. This is searched for recursively
    /// from the directory of the <see cref="BaseUnityPlugin"/> assembly.</param>
    /// <inheritdoc cref="LoadBundleFromPath(string, Action{PeakBundle}, ModDefinition?)"/>
    /// <param name="onLoaded"></param>
    /// <param name="loadContents"></param>
    private static void LoadBundleWithNameInternal(
        this BaseUnityPlugin baseUnityPlugin,
        string fileName,
        Action<PeakBundle> onLoaded,
        bool loadContents
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
            CorePlugin.Log.LogInfo($"Loading bundle at '{path}'...");
            _operations.Add(new LoadOperation(path, onLoaded, loadContents, modDefinition));
        }
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Does not register its contents automatically.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="LoadBundleWithName(BaseUnityPlugin, string, Action{PeakBundle})"/>
    /// over this method.
    /// </remarks>
    /// <inheritdoc cref="LoadBundleAndContentsFromPath(string, Action{PeakBundle}?, ModDefinition?)"/>
    public static void LoadBundleFromPath(
        string path,
        Action<PeakBundle> onLoaded,
        ModDefinition? mod = null
    )
    {
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(path);
        ThrowHelper.ThrowIfArgumentNull(onLoaded);

        CorePlugin.Log.LogInfo($"Loading bundle at '{path}'...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents: false, mod));
    }

    /// <summary>
    /// Load an AssetBundle async and get a callback for when it's loaded.
    /// Also registers its contents automatically.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="LoadBundleAndContentsWithName(BaseUnityPlugin, string, Action{PeakBundle})"/>
    /// over this method.
    /// </remarks>
    /// <param name="path">The absolute path to the AssetBundle.</param>
    /// <param name="onLoaded">Callback for when the AssetBundle is loaded.</param>
    /// <param name="mod">The <see cref="ModDefinition"/> that owns this <see cref="PeakBundle"/>.
    /// If this is set, the target asset bundle must not contain a <see cref="UnityModDefinition"/>
    /// <see cref="ScriptableObject"/>.</param>
    public static void LoadBundleAndContentsFromPath(
        string path,
        Action<PeakBundle>? onLoaded = null,
        ModDefinition? mod = null
    )
    {
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(path);

        CorePlugin.Log.LogInfo($"Loading bundle at '{path}'...");
        _operations.Add(new LoadOperation(path, onLoaded, loadContents: true, mod));
    }

    internal static IEnumerator LoadOperationsDeadlineWait(MonoBehaviour behaviour)
    {
        var maybeUI = SetupLoadingUI();
        if (maybeUI is not { } ui)
        {
            CorePlugin.Log.LogError("Loading UI failed!");
            while (_operations.Count > 0)
            {
                yield return null;
            }
            yield break;
        }

        var (text, disableLoadingUI) = ui;

        float startTime = Time.time;
        float lastUpdate = Time.time;
        while (_operations.Count > 0)
        {
            if (Time.time - lastUpdate <= 1)
            {
                yield return null;
            }

            // Only make the UI show up if it's taking a while.
            // This should make PEAKLib mostly invisible with light mods installed.
            if (Time.time - startTime > 3f)
            {
                text.gameObject.SetActive(true);
            }

            lastUpdate = Time.time;

            string bundlesWord = _operations.Count == 1 ? "bundle" : "bundles";
            text.text = $"PEAKLib: Waiting for {_operations.Count} {bundlesWord} to load...";

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

            yield return null;
        }

        disableLoadingUI();

        CorePlugin.Log.LogInfo("Bundle loading screen finished.");
    }

    private static (TMP_Text, Action)? SetupLoadingUI()
    {
        Canvas? canvas = GameObject.Find("LoadingScreenSimple(Clone)")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            CorePlugin.Log.LogError("Loading UI Canvas not found!");
            return null;
        }

        Transform? loadingTextTransform = canvas.transform.FindChildRecursive("LoadingText");
        if (loadingTextTransform == null)
        {
            CorePlugin.Log.LogError("Loading UI 'LoadingText' transform not found!");
            return null;
        }

        GameObject vanillaTextObj = loadingTextTransform.gameObject;

        GameObject textObj = Object.Instantiate(
            vanillaTextObj,
            vanillaTextObj.transform.parent,
            true
        );
        textObj.name = "PEAKLib Loading Text";

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = "";

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 300f);

        textObj.SetActive(false);

        return new (TextMeshProUGUI, Action)?(
            (
                text,
                () =>
                {
                    text.text = "PEAKLib: All bundles loaded!";
                    CorePlugin.Instance.StartCoroutine(WaitAndDisableText(textObj));
                }
            )
        );
    }

    static IEnumerator WaitAndDisableText(GameObject textObj)
    {
        yield return new WaitForSeconds(1f);

        if (textObj == null)
            yield break;

        textObj.SetActive(false);
    }

    private class LoadOperation
    {
        public string Path { get; }
        public DateTime StartTime { get; } = DateTime.Now;
        public State CurrentState { get; set; } = State.LoadingBundle;
        public bool LoadContents { get; }
        public ModDefinition? ModDefinition { get; }
        public Action<PeakBundle>? OnBundleLoaded { get; }
        public AssetBundleCreateRequest BundleRequest { get; }
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

        public enum State
        {
            LoadingBundle,
            LoadingContent,
        }

        internal LoadOperation(
            string path,
            Action<PeakBundle>? onBundleLoaded = null,
            bool loadContents = true,
            ModDefinition? modDefinition = null
        )
        {
            Path = path;
            OnBundleLoaded = onBundleLoaded;
            LoadContents = loadContents;
            ModDefinition = modDefinition;
            BundleRequest = AssetBundle.LoadFromFileAsync(path);
            BundleRequest.completed += OnLoaded;
        }

        private void OnLoaded(AsyncOperation operation) =>
            CorePlugin.Instance.StartCoroutine(FinishLoadOperation(this));

        private static IEnumerator FinishLoadOperation(LoadOperation operation)
        {
            var bundle = operation.BundleRequest.assetBundle;

            if (bundle == null)
            {
                CorePlugin.Log.LogError($"Failed to load bundle {operation.FileName}!");
                Finish();
                yield break;
            }

            operation.CurrentState = State.LoadingContent;
            var assetRequest = bundle.LoadAllAssetsAsync<ScriptableObject>();
            yield return assetRequest;

            Object[] assets = assetRequest.allAssets;

            List<ModDefinition> mods;
            try
            {
                mods = [.. assets.OfType<UnityModDefinition>().Select(x => x.Resolve())];
            }
            catch (Exception ex)
            {
                CorePlugin.Log.LogError($"Failed to Resolve ModDefinition: {ex}");
                Finish();
                yield break;
            }

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
                    CorePlugin.Log.LogError(
                        $"Bundle {operation.FileName} contains more than one mod!"
                    );
                    Finish();
                    yield break;
                default:
                    break;
            }

            var mod = mods[0];

            var contents = assets.OfType<IContent>();

            foreach (var content in contents)
            {
                mod.Content.Add(content);
            }

            if (operation.LoadContents)
            {
                foreach (var content in contents)
                {
                    try
                    {
                        mod.Register(content);
                    }
                    catch (Exception e)
                    {
                        CorePlugin.Log.LogError(
                            $"Failed to register '{mod.Id}:{content.Name}' ({content.GetType().Name}) from bundle '{operation.FileName}': {e}"
                        );
                    }
                }
            }

            var peakBundle = new PeakBundle(bundle, mod);

            operation.OnBundleLoaded?.SafeInvoke(peakBundle);
            BundleLoader.OnBundleLoaded?.SafeInvoke(peakBundle);

            // if (ConfigManager.ExtendedLogging.Value)
            // {
            CorePlugin.Log.LogInfo(
                $"Loaded bundle {operation.FileName} in {operation.ElapsedTime.TotalSeconds:N1}s"
            );
            // }

            Finish();

            if (_operations.Count == 0 && bundleLoadingWindowClosed && !calledOnBundleLoaded)
            {
                calledOnBundleLoaded = true;
                OnAllBundlesLoaded?.SafeInvoke();
            }

            void Finish()
            {
                _operations.Remove(operation);
            }
        }
    }
}
#endif
