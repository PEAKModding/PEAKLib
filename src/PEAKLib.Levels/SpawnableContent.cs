using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PEAKLib.Core;
using PEAKLib.Levels.Core;
using UnityEngine;

namespace PEAKLib.Levels;

// TODO: Implement this.

/// <summary>
/// A registerable PEAKLib spawnable object.
/// </summary>
public class SpawnableContent : IContent<SpawnableContent>
{
    /// <summary>
    /// The name of this segment.
    /// </summary>
    public string Name => SpawnableName;

    [Tooltip("Registry name other bundles will use to reference this spawnable")]
    public string SpawnableName = string.Empty;

    [Tooltip("Spawnable prefab")]
    public GameObject Prefab = null!;
    internal static List<RegisteredContent<SpawnableContent>> s_RegisteredSpawnables = [];

    /// <inheritdoc/>
    public RegisteredContent<SpawnableContent> Register(ModDefinition owner)
    {
        Process(owner);
        var registered = ContentRegistry.Register(this, owner);
        s_RegisteredSpawnables.Add(registered);
        return registered;
    }

    /// <inheritdoc/>
    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;

    void Process(ModDefinition mod)
    {
        var spawn = this;
        var spawnName = spawn.SpawnableName?.Trim();
        if (string.IsNullOrEmpty(spawnName))
            return;

        var bundlePath = mod.Id;
        var prefabName = Prefab.name;

        if (!string.IsNullOrEmpty(prefabName))
        {
            // NOTE: I don't really know how this is supposed to work, and it might
            // not work as intended due to my changes.
            PatchedContent.SpawnableRegistry.Registry[spawnName] =
                new PatchedContent.SpawnableEntry(bundlePath, prefabName);
            Debug.Log(
                $"[SpawnableRegistrar] Registered ExtendedSpawnable '{spawnName}' => {bundlePath} :: {prefabName}"
            );
        }
    }
}
