#if !UNITY_EDITOR
using System.Xml.Linq;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;
using PEAKLib.Networking.Modules;
using PEAKLib.Networking.Services;
using UnityEngine;

namespace PEAKLib.Networking;

/// <summary>
/// BepInEx plugin of PEAKLib.Networking.
/// Depend on this with <c>[BepInDependency(NetworkingPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class NetworkingPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    /// <summary>
    /// </summary>
    public static INetworkingService? Service { get; private set; }
    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(NetworkingPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");

        var svc = new SteamNetworkingService();
        Service = svc;
        svc.Initialize();
        var go = new GameObject("PEAKLib.Networking.Poller");
        go.AddComponent<NetworkingPoller>().hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(go);
    }

    private void OnDestroy()
    {
        Service?.Shutdown();
    }
}
#endif