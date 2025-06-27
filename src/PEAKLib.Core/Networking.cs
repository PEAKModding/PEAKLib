using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using Photon.Pun;
using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// Handles Networking.
/// </summary>
public static class Networking
{
    /// <summary>
    /// The Player's network manager reference.
    /// </summary>
    public static NetworkManager? networkManager;

    /// <summary>
    /// Check if the host has plugin matching the given guid.
    /// </summary>
    public static void RequestHostHasPlugin(string pluginGuid, Action onHasPlugin, Action? onNoPlugin = null)
    {
        if (networkManager == null)
        {
            CorePlugin.Log.LogError("Local Networking Component could not be found!");
            return;
        }

        if (networkManager.hostGuids.IndexOf(pluginGuid) != -1)
        {
            CorePlugin.Log.LogInfo($"Host has plugin with guid: {pluginGuid}");
            onHasPlugin();
        }
        else
        {
            CorePlugin.Log.LogInfo($"Host does not have plugin with guid: {pluginGuid}");
            if (onNoPlugin != null) onNoPlugin();
        }
    }
}

/// <summary>
/// The Component for Networking added to the character.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    private Character? character;

    /// <summary>
    /// List of the host client's installed plugins
    /// </summary>
    public List<string> hostGuids = new List<string>();

    private void Awake()
    {
        CorePlugin.Log.LogInfo("Loaded Network Component");
        character = GetComponent<Character>();

        if (character == null)
        {
            CorePlugin.Log.LogError("Could not find reference to local character!");
            return;
        }

        CallRPC("RequestHostPlugins_Rpc", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RequestHostPlugins_Rpc()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int pluginCount = Chainloader.PluginInfos.Count;

        System.Object[] guids = new System.Object[pluginCount];

        int index = 0;
        foreach (var plugin in Chainloader.PluginInfos)
        {
            guids[index] = plugin.Value.Metadata.GUID;
            index += 1;
        }

        CallRPC("HostPlugins_Rpc", RpcTarget.All, guids);
    }

    [PunRPC]
    private void HostPlugins_Rpc(System.Object[] guids)
    {
        hostGuids.Clear();

        foreach (String guid in guids)
        {
            CorePlugin.Log.LogInfo($"Recieved Plugin guid from host: {guid}");
            hostGuids.Add(guid);
        }
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
        CallRPC("RequestHostPlugins_Rpc", RpcTarget.MasterClient);
    }
}