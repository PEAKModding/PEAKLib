using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

// Taken from https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Objects/CustomPrefabPool.cs

namespace PEAKLib.Core;

internal class CustomPrefabPool : IPunPrefabPool
{
    internal readonly Dictionary<string, GameObject> idToGameObject = [];
    internal readonly Dictionary<GameObject, string> gameObjectToId = [];

    public DefaultPool DefaultPool
    {
        get
        {
            _defaultPool ??= new DefaultPool();
            return _defaultPool;
        }
        set
        {
            if (value != null)
            {
                _defaultPool = value;
            }
        }
    }

    private DefaultPool? _defaultPool;

    internal CustomPrefabPool() { }

    /// <summary>
    /// Registers a prefab with the prefab pool for later use
    /// </summary>
    /// <param name="prefabId">A string ID for the prefab</param>
    /// <param name="prefab">The GameObject used for instantiating</param>
    /// <returns>
    /// Returns <b>TRUE</b> if the prefab is successfully registered.
    /// Returns <b>FALSE</b> if the prefab is already registered or a vanilla prefab exists with the same ID
    /// </returns>
    /// <remarks>
    /// This method utilizes <see cref="CorePlugin.Log"/>. Any registrations must take place
    /// <b>after</b> the standard <em>BaseUnityPlugin.Awake</em> or PeakLib must be added as a BepInExDependency
    /// to avoid issues with potential null references 
    /// </remarks>
    public bool TryRegisterPrefab(string prefabId, GameObject prefab)
    {
        prefabId = ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(prefabId);

        if (HasPrefab(prefabId))
        {
            CorePlugin.Log.LogError(
                $"CustomPrefabPool: failed to register network prefab \"{prefabId}\". Prefab already exists in Resources with the same prefab id."
            );
            return false;
        }

        if (idToGameObject.ContainsKey(prefabId))
        {
            CorePlugin.Log.LogError(
                $"CustomPrefabPool: failed to register network prefab \"{prefabId}\". There is already a prefab registered with the same prefab id."
            );
            return false;
        }

        idToGameObject.Add(prefabId, prefab);
        gameObjectToId.Add(prefab, prefabId);

        CorePlugin.Log.LogDebug($"CustomPrefabPool: registered network prefab \"{prefabId}\"");
        return true;
    }

    public bool HasPrefab(GameObject prefab) => idToGameObject.ContainsValue(prefab);

    public bool HasPrefab(string prefabId) => TryGetPrefab(prefabId, out _);

    public bool TryGetPrefabId(GameObject prefab, [NotNullWhen(true)] out string? id) =>
        gameObjectToId.TryGetValue(prefab, out id);

    public bool TryGetPrefab(string prefabId, [NotNullWhen(true)] out GameObject? prefab)
    {
        prefabId = ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(prefabId);

        if (idToGameObject.TryGetValue(prefabId, out prefab))
        {
            return true;
        }

        prefab = Resources.Load<GameObject>(prefabId);
        return prefab != null;
    }

    public GameObject? Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        prefabId = ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(prefabId);

        GameObject result;

        if (!idToGameObject.TryGetValue(prefabId, out GameObject? prefab))
        {
            result = DefaultPool.Instantiate(prefabId, position, rotation);

            if (result == null)
            {
                CorePlugin.Log.LogError(
                    $"CustomPrefabPool: failed to spawn network prefab \"{prefabId}\". GameObject is null."
                );
            }

            return result;
        }

        bool activeSelf = prefab.activeSelf;

        if (activeSelf)
        {
            prefab.SetActive(false);
        }

        result = Object.Instantiate(prefab, position, rotation);

        if (activeSelf)
        {
            prefab.SetActive(true);
        }

        CorePlugin.Log.LogInfo(
            $"CustomPrefabPool: spawned network prefab \"{prefabId}\" at position {position}, rotation {rotation.eulerAngles}"
        );

        return result;
    }

    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }
}
