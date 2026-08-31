using System;
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
    public RegisteredContent<ItemContent> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);

        var modItemComponent = item.GetComponent<ModItemComponent>();
        if (modItemComponent != null)
            modItemComponent.InitializeModItem(owner);

        InitTranslations();

#if !UNITY_EDITOR
        NetworkPrefabManager.RegisterNetworkPrefab(owner, "0_Items/", item.gameObject);
        s_RegisteredItems.Add(registered);
        ItemRegistrar.RegisterIfTooLate(registered);
#endif
        return registered;
    }

    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    // Without this, item names and stuff will start with "LOC:"
    void InitTranslations()
    {
        var languages = (LocalizedText.Language[])Enum.GetValues(typeof(LocalizedText.Language));

        InitNameTranslations(languages);
        InitMainInteractTranslations(languages);
        InitSecondaryInteractTranslations(languages);
    }

    void InitNameTranslations(LocalizedText.Language[] languages)
    {
        var name = Item.UIData.itemName;
        var nameIndex = LocalizedText.GetNameIndex(name.ToUpperInvariant());

        if (LocalizedText.mainTable.ContainsKey(nameIndex))
            return;

        List<string> nameList = new(capacity: languages.Length);
        for (int i = 0; i < languages.Length; i++)
        {
            nameList.Add(name);
        }

        LocalizedText.mainTable.Add(nameIndex, nameList);
    }

    void InitMainInteractTranslations(LocalizedText.Language[] languages)
    {
        var mainInteract = Item.UIData.mainInteractPrompt;
        var mainInteractUpper = mainInteract.ToUpperInvariant();

        if (LocalizedText.mainTable.ContainsKey(mainInteractUpper))
            return;

        List<string> mainInteractList = new(capacity: languages.Length);

        for (int i = 0; i < languages.Length; i++)
        {
            mainInteractList.Add(mainInteract);
        }

        LocalizedText.mainTable.Add(mainInteractUpper, mainInteractList);
    }

    void InitSecondaryInteractTranslations(LocalizedText.Language[] languages)
    {
        var secondaryInteract = Item.UIData.secondaryInteractPrompt;
        var secondaryInteractUpper = secondaryInteract.ToUpperInvariant();

        if (LocalizedText.mainTable.ContainsKey(secondaryInteractUpper))
            return;

        List<string> secondaryInteractList = new(capacity: languages.Length);

        for (int i = 0; i < languages.Length; i++)
        {
            secondaryInteractList.Add(secondaryInteract);
        }

        LocalizedText.mainTable.Add(secondaryInteractUpper, secondaryInteractList);
    }

    /// <inheritdoc/>
    public IContent Resolve() => this;

    /// <inheritdoc/>
    IEnumerable<GameObject> IGameObjectContent.EnumerateGameObjects()
    {
        yield return Item.gameObject;
    }
}
