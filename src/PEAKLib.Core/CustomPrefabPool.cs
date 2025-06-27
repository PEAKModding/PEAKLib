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
