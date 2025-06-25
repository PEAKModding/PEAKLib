using System;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Core.UnityEditor;

/// <summary>
/// A <see cref="ScriptableObject"/> representation of a <see cref="ModDefinition"/>.
/// </summary>
[CreateAssetMenu(
    fileName = "UnityModDefinition",
    menuName = "PEAKLib/UnityModDefinition",
    order = 0
)]
public class UnityModDefinition : ScriptableObject, IModDefinitionResolvable
{
    [SerializeField]
    private string modId = "";

    [SerializeField]
    private string modName = "";

    [SerializeField]
    private string modVersion = "";

    [SerializeField]
    private List<IContent> content = [];

    /// <exception cref="FormatException"></exception>
    /// <inheritdoc/>
    public ModDefinition Resolve()
    {
        ThrowHelper.ThrowIfFieldNullOrWriteSpace(modId);
        ThrowHelper.ThrowIfFieldNullOrWriteSpace(modName);
        ThrowHelper.ThrowIfFieldNullOrWriteSpace(modVersion);

        Version version;
        try
        {
            version = new Version(modVersion);
        }
        catch (FormatException ex)
        {
            throw new FormatException(
                $"Version of the mod is not in a valid format!\n{ex.Message}",
                ex
            );
        }

        var modDefinition = ModDefinition.GetOrCreate(modId, modName, version);

        foreach (var modContent in content)
        {
            modDefinition.Content.Add(modContent);
        }

        return modDefinition;
    }
}
