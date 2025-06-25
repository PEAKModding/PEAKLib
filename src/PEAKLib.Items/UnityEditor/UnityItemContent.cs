using System.Collections.Generic;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Items.UnityEditor;

/// <summary>
/// A <see cref="ScriptableObject"/> representation of <see cref="IItemContent"/>.
/// </summary>
[CreateAssetMenu(fileName = "ItemContent", menuName = "PEAKLib/ItemContent", order = 1)]
public class UnityItemContent : ScriptableObject, IItemContent
{
    /// <inheritdoc/>
    [field: SerializeField]
    public Item Item { get; } = null!;
    internal static readonly Dictionary<UnityItemContent, ItemContent> s_UnityToModItem = [];

    /// <inheritdoc/>
    public IRegisteredContent Register(ModDefinition owner) => Resolve().Register(owner);

    /// <inheritdoc/>
    public ItemContent Resolve()
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

    IContent IContent.Resolve() => Resolve();
}
