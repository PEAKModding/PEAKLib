#if !UNITY_EDITOR
using System;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core.Hooks;
using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// BepInEx plugin of PEAKLib.Core.
/// Depend on this with <c>[BepInDependency(CorePlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
public partial class CorePlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);
    internal static CorePlugin Instance =>
        _instance
        ?? throw new NullReferenceException(
            "PEAKLib.Core hasn't been initialized yet! "
                + "Please depend on it with [BepInDependency(CorePlugin.Id)]"
        );

    private static CorePlugin? _instance = null;

    private void Awake()
    {
        _instance = this;
        MonoDetourManager.InvokeHookInitializers(typeof(CorePlugin).Assembly);

        CharacterRegistrationHooks.OnCharacterAdded += (Character character) =>
        {
            Networking.s_NetworkManager = character.gameObject.AddComponent<NetworkManager>();
        };

        BundleLoader.LoadAllBundles(Paths.PluginPath, ".autoload.peakbundle");
        BundleLoader.LoadAllBundles(Paths.PluginPath, ".autoload_peakbundle");

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void Start()
    {
        BundleLoader.CloseBundleLoadingWindow();
    }
}
#endif
