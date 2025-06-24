using System;
using System.Collections.Generic;
using PEAKLib.Core;

namespace PEAKLib.Items;

public class ModItem(Item item) : IModContent<ModItem>
{
    public Item Item { get; } = ThrowHelper.ThrowIfArgumentNull(item);
    internal static List<RegisteredModContent<ModItem>> s_RegisteredItems = [];

    /// <inheritdoc/>
    public RegisteredModContent<ModItem> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);
        NetworkPrefabManager.RegisterNetworkPrefab(owner, item.gameObject);
        s_RegisteredItems.Add(registered);
        return registered;
    }

    IRegisteredModContent IModContent.Register(ModDefinition owner) => Register(owner);
}
