using System.Collections.Generic;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Items.UnityEditor;

/// <summary>
/// A <see cref="ScriptableObject"/> representation of <see cref="IModItem"/>.
/// </summary>
[CreateAssetMenu(fileName = "UnityModItem", menuName = "PEAKLib/UnityModItem", order = 0)]
public class UnityModItem : ScriptableObject, IModItem
{
    /// <inheritdoc/>
    [field: SerializeField]
    public Item Item { get; } = null!;
    internal static readonly Dictionary<UnityModItem, ModItem> s_UnityToModItem = [];

    /// <inheritdoc/>
    public IRegisteredModContent Register(ModDefinition owner) => Resolve().Register(owner);

    /// <inheritdoc/>
    public ModItem Resolve()
    {
        ThrowHelper.ThrowIfFieldNull(Item);

        if (s_UnityToModItem.TryGetValue(this, out var modItem))
        {
            return modItem;
        }

        modItem = new(Item);
        s_UnityToModItem.Add(this, modItem);

        return modItem;
    }

    IModContent IModContent.Resolve() => Resolve();
}
