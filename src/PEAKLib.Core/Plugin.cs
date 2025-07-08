#if !UNITY_EDITOR
using System;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
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

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(CorePlugin).Assembly);

        PlayerHandler.OnCharacterRegistered += (Character character) =>
        {
            Networking.s_NetworkManager = character.gameObject.AddComponent<NetworkManager>();
        };

        BundleLoader.LoadAllBundles(Paths.PluginPath, ".autoload.peakbundle");

        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
#endif
