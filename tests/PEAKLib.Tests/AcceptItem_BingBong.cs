using PEAKLib.Items.ItemAcceptor;
using UnityEngine;

namespace PEAKLib.Items;

/// <summary>
/// Bingbong accepts items and poisons you
/// </summary>
public class AcceptItem_BingBong : MonoBehaviour, IItemAcceptor
{
    public bool SecondaryInteractOnly => false;

    public void AcceptItem(Item item, Character interactor)
    {
        string thisItemName = GetComponent<Item>()?.GetItemName() ?? "Item";
        TestsPlugin.Log.LogInfo($"{thisItemName} accepted {item.GetItemName()} from {interactor.characterName}");
        interactor.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Poison, 0.1f);
        IItemAcceptor.ConsumeOneUse(item);
    }

    public string GetPrompt()
    {
        Item item = Character.localCharacter.data.currentItem;
        if (item?.canUseOnFriend ?? false)
        {
            string prompt = item.UIData.secondaryInteractPrompt.Replace("#targetChar", "").Trim();
            return $"{prompt} Bing Bong: {item.GetItemName()}";
        }
        return "";
    }

    public bool AllowInteraction() => true;
}
