using System;
using System.Diagnostics.CodeAnalysis;
using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// An <see cref="ItemComponent"/> to use with PEAKLib.
/// </summary>
public abstract class ModItemComponent : ItemComponent
{
    /// <summary>
    /// The <see cref="ModDefinition"/> which owns this item.
    /// </summary>
    public ModDefinition Mod =>
        _mod
        ?? throw new NullReferenceException(
            $"{nameof(ModItemComponent)}'s {nameof(Mod)} property was null"
                + " meaning it wasn't initialized by PEAKLib!"
        );

    private ModDefinition? _mod;

    /// <summary>
    /// Used by PEAKLib to initialize this <see cref="ModItemComponent"/>
    /// during <see cref="ItemContent"/> Registration.
    /// </summary>
    /// <param name="mod">The <see cref="ModDefinition"/> who registered this item.</param>
    internal void InitializeModItem(ModDefinition mod)
    {
        _mod = ThrowHelper.ThrowIfArgumentNull(mod);
    }

    /// <summary>
    /// Tries to get raw mod item data for this item.
    /// </summary>
    /// <param name="rawData">The data as a byte array or null.</param>
    /// <returns>Whether or not data was found.</returns>
    public bool TryGetRawModItemData([NotNullWhen(true)] out byte[]? rawData) =>
        CustomItemData.TryGetRawModItemData(this, Mod, out rawData);

    /// <summary>
    /// Sets raw mod item data for this item.
    /// </summary>
    /// <param name="rawData">The data to set as a byte array.</param>
    public void SetRawModItemData(byte[] rawData) =>
        CustomItemData.SetRawModItemData(this, Mod, rawData);
}
