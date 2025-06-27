using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using Photon.Pun;

namespace PEAKLib.Core;

/// <summary>
/// Handles Networking.
/// </summary>
public static class Networking
{
    /// <summary>
    /// List of the host client's installed plugins as GUIDs.
    /// </summary>
    public static IReadOnlyCollection<string> HostPluginGuids => hostPluginGuids;
    internal static readonly HashSet<string> hostPluginGuids = [];
    static readonly Dictionary<
        string,
        (Action HasPlugin, Action NoPlugin)
    > s_OnHasPluginCallbacks = [];

    /// <summary>
    /// The Player's network manager reference.
    /// </summary>
    internal static NetworkManager? s_NetworkManager;

    /// <summary>
    /// Add persistent listener callbacks for whenever a new host is set,
    /// and every time query the host's plugin GUIDs for the specified
    /// <paramref name="pluginGuid"/> and run the appropriate callback.<br/>
    /// <br/>
    /// This is useful for when your plugin can be considered a cheat mod so
    /// it can still be used but is restricted for when the host doesn't have it
    /// installed.
    /// </summary>
    /// <remarks>
    /// A new host can be set multiple times in a game, so your mod should
    /// take this into account and be able to patch/unpatch itself when needed.
    /// </remarks>
    /// <param name="pluginGuid">The plugin GUID to query.</param>
    /// <param name="onHasPlugin">A callback for when the host has the specified plugin.</param>
    /// <param name="onNoPlugin">A callback for when the host doesn't have the specified plugin.</param>
    public static void AddHostHasPluginListers(
        string pluginGuid,
        Action onHasPlugin,
        Action onNoPlugin
    )
    {
        ThrowHelper.ThrowIfArgumentNull(pluginGuid);
        ThrowHelper.ThrowIfArgumentNull(onHasPlugin);
        ThrowHelper.ThrowIfArgumentNull(onNoPlugin);

        if (s_OnHasPluginCallbacks.TryGetValue(pluginGuid, out var callbacks))
        {
            callbacks.HasPlugin += onHasPlugin;
            callbacks.NoPlugin += onNoPlugin;
            return;
        }

        s_OnHasPluginCallbacks.Add(pluginGuid, (onHasPlugin, onNoPlugin));
    }

    internal static void InvokeGuidCallbacks()
    {
        foreach (var pluginGuid in s_OnHasPluginCallbacks)
        {
            if (hostPluginGuids.Contains(pluginGuid.Key))
            {
                CorePlugin.Log.LogDebug($"Host has plugin with guid: {pluginGuid}");
                foreach (var onHasPlugin in pluginGuid.Value.HasPlugin.GetInvocationList())
                {
                    try
                    {
                        ((Action)onHasPlugin)();
                    }
                    catch (Exception ex)
                    {
                        CorePlugin.Log.LogError($"Unhandled exception in callback: {ex}");
                    }
                }
            }
            else
            {
                CorePlugin.Log.LogDebug($"Host does not have plugin with guid: {pluginGuid}");
                foreach (var onNoPlugin in pluginGuid.Value.NoPlugin.GetInvocationList())
                {
                    try
                    {
                        ((Action)onNoPlugin)();
                    }
                    catch (Exception ex)
                    {
                        CorePlugin.Log.LogError($"Unhandled exception in callback: {ex}");
                    }
                }
            }
        }
    }
}

/// <summary>
/// The Component for Networking added to the character.
/// </summary>
internal class NetworkManager : MonoBehaviourPunCallbacks
{
    private Character? character;

    private void Awake()
    {
        CorePlugin.Log.LogDebug("Loaded Network Component");
        character = GetComponent<Character>();

        if (character == null)
        {
            CorePlugin.Log.LogError("Could not find reference to local character!");
            return;
        }

        CallRPC(nameof(GetHostPluginsRPC), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void GetHostPluginsRPC()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        string[] guids = [.. Chainloader.PluginInfos.Select(x => x.Value.Metadata.GUID)];

        CallRPC(nameof(ReceivePluginsFromHostRPC), RpcTarget.All, guids);
    }

    [PunRPC]
    private void ReceivePluginsFromHostRPC(string[] guids)
    {
        Networking.hostPluginGuids.Clear();

        foreach (string guid in guids)
        {
            CorePlugin.Log.LogDebug($"Received Plugin guid from host: {guid}");
            Networking.hostPluginGuids.Add(guid);
        }

        Networking.InvokeGuidCallbacks();
    }

    private void CallRPC(string methodName, RpcTarget target, params object[] parameters)
    {
        if (character == null)
        {
            CorePlugin.Log.LogError("Could not find reference to local character!");
            return;
        }

        character.view.RPC(methodName, target, parameters);
    }

    /// <summary>
    /// Update Networking when the master client Switches
    /// </summary>
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        CorePlugin.Log.LogInfo($"Master Client Switched to player: {newMasterClient.NickName}");
        CallRPC(nameof(GetHostPluginsRPC), RpcTarget.MasterClient);
    }
}
