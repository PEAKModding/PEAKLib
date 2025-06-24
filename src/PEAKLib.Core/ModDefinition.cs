using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx;

namespace PEAKLib.Core;

/// <summary>
/// A representation of a mod PEAKLib understands.
/// </summary>
public class ModDefinition
{
    /// <summary>
    /// The Id of this <see cref="ModDefinition"/>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of this <see cref="ModDefinition"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the version of this <see cref="ModDefinition"/>.
    /// </summary>
    public Version Version { get; }

    public HashSet<IModContent> Content { get; }

    static readonly Dictionary<string, ModDefinition> s_guidToMod = [];

    private ModDefinition(string id, string name, Version version)
    {
        Id = ThrowHelper.ThrowIfArgumentNull(id);
        Name = ThrowHelper.ThrowIfArgumentNull(name);
        Version = ThrowHelper.ThrowIfArgumentNull(version);
    }

    /// <summary>
    /// Creates a new <see cref="ModDefinition"/> for the <paramref name="pluginInfo"/>
    /// or returns it if it already exists.
    /// </summary>
    /// <param name="pluginInfo">The <see cref="PluginInfo"/> whose
    /// <see cref="ModDefinition"/> to create or get.</param>
    /// <inheritdoc cref="GetOrCreate(string, string, System.Version)"/>
    public static ModDefinition GetOrCreate(PluginInfo pluginInfo) =>
        GetOrCreate(pluginInfo.Metadata);

    /// <summary>
    /// Creates a new <see cref="ModDefinition"/> for the <paramref name="bepInPlugin"/>
    /// or returns it if it already exists.
    /// </summary>
    /// <param name="bepInPlugin">The <see cref="BepInPlugin"/> whose
    /// <see cref="ModDefinition"/> to create or get.</param>
    /// <inheritdoc cref="GetOrCreate(string, string, System.Version)"/>
    public static ModDefinition GetOrCreate(BepInPlugin bepInPlugin) =>
        GetOrCreate(bepInPlugin.GUID, bepInPlugin.Name, bepInPlugin.Version);

    /// <summary>
    /// Creates a new <see cref="ModDefinition"/> for the <paramref name="id"/>
    /// or returns an existing one if it exists already.
    /// </summary>
    /// <param name="id">The ID/GUID of the mod.</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="version">The version of the mod.</param>
    /// <returns>A new or existing <see cref="ModDefinition"/>.</returns>
    public static ModDefinition GetOrCreate(string id, string name, Version version)
    {
        if (s_guidToMod.TryGetValue(id, out var modDefinition))
        {
            return modDefinition;
        }

        modDefinition = new(id, name, version);
        s_guidToMod.Add(id, modDefinition);

        return modDefinition;
    }

    /// <summary>
    /// Tries to get a <see cref="ModDefinition"/> with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID/GUID of the mod to god.</param>
    /// <param name="modDefinition">The found <see cref="ModDefinition"/> or null.</param>
    /// <returns>Whether or not the <see cref="ModDefinition"/> was found.</returns>
    public static bool TryGetMod(string id, [NotNullWhen(true)] out ModDefinition? modDefinition) =>
        s_guidToMod.TryGetValue(id, out modDefinition);

    public void RegisterContent()
    {
        foreach (var modContent in Content)
        {
            if (modContent.IsRegistered())
            {
                continue;
            }

            try
            {
                modContent.Register(this);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }
    }
}
