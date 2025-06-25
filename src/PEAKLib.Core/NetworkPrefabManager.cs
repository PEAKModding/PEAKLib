using System;
using System.Diagnostics.CodeAnalysis;
using Photon.Pun;
using UnityEngine;

// Taken from https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkPrefabs.cs

namespace PEAKLib.Core;

/// <summary>
/// Handles Network prefabs.
/// </summary>
public static class NetworkPrefabManager
{
    internal static CustomPrefabPool s_CustomPrefabPool
    {
        get
        {
            _customPrefabPool ??= new CustomPrefabPool();
            return _customPrefabPool;
        }
        private set { _customPrefabPool = value; }
    }

    private static CustomPrefabPool? _customPrefabPool;

    internal static void Initialize()
    {
        if (PhotonNetwork.PrefabPool is CustomPrefabPool)
        {
            CorePlugin.Log.LogWarning(
                "NetworkPrefabs failed to initialize. PhotonNetwork.PrefabPool is already a CustomPrefabPool."
            );
            return;
        }

        CorePlugin.Log.LogInfo($"Initializing NetworkPrefabs.");
        CorePlugin.Log.LogDebug($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");

        if (PhotonNetwork.PrefabPool is DefaultPool defaultPool)
        {
            s_CustomPrefabPool.DefaultPool = defaultPool;
        }
        else if (PhotonNetwork.PrefabPool is not CustomPrefabPool)
        {
            CorePlugin.Log.LogWarning(
                $"PhotonNetwork has an unknown prefab pool assigned. PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}"
            );
        }

        PhotonNetwork.PrefabPool = s_CustomPrefabPool;

        CorePlugin.Log.LogInfo("Replaced PhotonNetwork.PrefabPool with CustomPrefabPool.");
        CorePlugin.Log.LogDebug($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");
        CorePlugin.Log.LogInfo($"Finished initializing NetworkPrefabs.");
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="mod">The mod who owns this content.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(ModDefinition mod, GameObject prefab)
    {
        RegisterNetworkPrefab(mod, "", prefab);
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="mod">The mod who owns this content.</param>
    /// <param name="folder">The folder in Resources that the prefab should be in.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(ModDefinition mod, string folder, GameObject prefab)
    {
        ThrowHelper.ThrowIfArgumentNull(mod);
        // Rename the prefab to include the mod ID. This is to prevent conflicts with other mods.
        // We have to do it this way since the finding items using the prefab name is hardcoded.
        prefab.name = $"{mod.Id}:{prefab.name}";
        RegisterNetworkPrefab(folder + prefab.name, prefab);
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(string prefabId, GameObject prefab)
    {
        if (s_CustomPrefabPool.TryRegisterPrefab(prefabId, prefab))
        {
            return;
        }

        throw new Exception("Failed to register network prefab!");
    }

    /// <summary>
    /// Tries to register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static bool TryRegisterNetworkPrefab(string prefabId, GameObject prefab) =>
        s_CustomPrefabPool.TryRegisterPrefab(prefabId, prefab);

    /// <summary>
    /// Check if a <see cref="GameObject"/> with the specified ID is a network prefab.
    /// </summary>
    /// <param name="prefabId">The <see cref="GameObject"/> ID to check.</param>
    /// <returns>Whether or not the <see cref="GameObject"/> is a network prefab.</returns>
    public static bool HasNetworkPrefab(string prefabId) => s_CustomPrefabPool.HasPrefab(prefabId);

    /// <summary>
    /// Tries to get a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/>.</param>
    /// <param name="prefab">The found <see cref="GameObject"/> or null.</param>
    /// <returns>Whether or not the <paramref name="prefab"/> was found.</returns>
    public static bool TryGetNetworkPrefab(
        string prefabId,
        [NotNullWhen(true)] out GameObject? prefab
    ) => s_CustomPrefabPool.TryGetPrefab(prefabId, out prefab);

    /// <summary>
    /// Spawns a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/> to spawn.</param>
    /// <param name="position">The position where the <see cref="GameObject"/> will be spawned.</param>
    /// <param name="rotation">The rotation of the <see cref="GameObject"/>.</param>
    /// <param name="group">The interest group. See: https://doc.photonengine.com/pun/current/gameplay/interestgroups</param>
    /// <param name="data">Custom instantiation data. See: https://doc.photonengine.com/pun/current/gameplay/instantiation#custom-instantiation-data</param>
    /// <returns>The spawned <see cref="GameObject"/> or null.</returns>
    public static GameObject SpawnNetworkPrefab(
        string prefabId,
        Vector3 position,
        Quaternion rotation,
        byte group = 0,
        object[]? data = null
    )
    {
        ThrowHelper.ThrowIfArgumentNullOrWriteSpace(prefabId);

        if (!HasNetworkPrefab(prefabId))
        {
            throw new Exception(
                $"Failed to spawn network prefab \"{prefabId}\". PrefabId is not registered as a network prefab."
            );
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            throw new Exception(
                $"Failed to spawn network prefab \"{prefabId}\". You are not the host."
            );
        }

        return PhotonNetwork.InstantiateRoomObject(prefabId, position, rotation, group, data);
    }
}
