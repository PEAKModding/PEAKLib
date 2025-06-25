using System.Collections.Generic;
using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// A PEAKLib <see cref="ModItem"/>.
/// </summary>
public class ModItem(Item item) : IModContent<ModItem>, IModItem
{
    /// <inheritdoc/>
    public Item Item { get; } = ThrowHelper.ThrowIfArgumentNull(item);
    internal static List<RegisteredModContent<ModItem>> s_RegisteredItems = [];

    /// <inheritdoc/>
    public RegisteredModContent<ModItem> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);
        NetworkPrefabManager.RegisterNetworkPrefab(owner, "0_Items/", item.gameObject);
        s_RegisteredItems.Add(registered);
        return registered;
    }

    IRegisteredModContent IModContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IModContent Resolve() => this;
}
