using System.Collections.Generic;
using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// A PEAKLib <see cref="ItemContent"/>.
/// </summary>
public class ItemContent(Item item) : IContent<ItemContent>, IItemContent
{
    /// <inheritdoc/>
    public Item Item { get; } = ThrowHelper.ThrowIfArgumentNull(item);
    internal static List<RegisteredContent<ItemContent>> s_RegisteredItems = [];

    /// <inheritdoc/>
    public RegisteredContent<ItemContent> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);
        NetworkPrefabManager.RegisterNetworkPrefab(owner, "0_Items/", item.gameObject);
        s_RegisteredItems.Add(registered);
        return registered;
    }

    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;
}
