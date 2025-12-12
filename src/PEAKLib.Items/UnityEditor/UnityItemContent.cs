using System;
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
    public string Name
    {
        get
        {
            if (ItemPrefab == null)
                return name;

            var item = ItemPrefab.GetComponent<Item>();
            if (item == null)
                return name;

            return item.name;
        }
    }

    /// <inheritdoc/>
    public Item Item => Resolve().Item;

    /// <inheritdoc/>
    public Component Component => Item;

    /// <summary>
    /// The prefab that contains an <see cref="global::Item"/> component.
    /// </summary>
    [field: SerializeField]
    public GameObject ItemPrefab { get; private set; } = null!;

    internal static readonly Dictionary<UnityItemContent, ItemContent> s_UnityToModItem = [];

    /// <inheritdoc/>
    public IRegisteredContent Register(ModDefinition owner) => Resolve().Register(owner);

    /// <inheritdoc/>
    public ItemContent Resolve()
    {
        if (s_UnityToModItem.TryGetValue(this, out var modItem))
        {
            return modItem;
        }

        var item =
            ThrowHelper.ThrowIfFieldNull(ItemPrefab).GetComponent<Item>()
            ?? throw new NullReferenceException(
                $"{nameof(Item)} component on {nameof(ItemPrefab)} is null!"
            );

        modItem = new(item);
        s_UnityToModItem.Add(this, modItem);

        return modItem;
    }

    IContent IContent.Resolve() => Resolve();
}
