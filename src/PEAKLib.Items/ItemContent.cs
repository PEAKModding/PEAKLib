using System.Collections.Generic;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Items;

/// <summary>
/// A PEAKLib <see cref="ItemContent"/>.
/// </summary>
public class ItemContent(Item item) : IContent<ItemContent>, IItemContent
{
    internal static List<RegisteredContent<ItemContent>> s_RegisteredItems = [];

    /// <inheritdoc/>
    public string Name => Item.name;

    /// <inheritdoc/>
    public Item Item { get; } = ThrowHelper.ThrowIfArgumentNull(item);

    /// <inheritdoc/>
    public Component Component => Item;

    /// <inheritdoc/>
    public RegisteredContent<ItemContent> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);

        var modItemComponent = item.GetComponent<ModItemComponent>();
        if (modItemComponent != null)
            modItemComponent.InitializeModItem(owner);

#if !UNITY_EDITOR
        NetworkPrefabManager.RegisterNetworkPrefab(owner, "0_Items/", item.gameObject);
        s_RegisteredItems.Add(registered);
        ItemRegistrar.RegisterIfTooLate(registered);
#endif
        return registered;
    }

    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;

    /// <inheritdoc/>
    IEnumerable<GameObject> IGameObjectContent.EnumerateGameObjects()
    {
        yield return Item.gameObject;
    }
}
