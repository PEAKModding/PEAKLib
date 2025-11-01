using PEAKLib.Items.ItemAcceptor;
using UnityEngine;

namespace PEAKLib.Tests;

/// <summary>
/// Second bingbong item acceptor, for testing
/// Makes player drowsy when you use a healing item on it
/// </summary>
public class AcceptItem_BingBong2 : MonoBehaviour, IItemAcceptor
{
    public bool SecondaryInteractOnly => false;

    public void AcceptItem(Item item, Character interactor)
    {
        string thisItemName = GetComponent<Item>()?.GetItemName() ?? "Item";
        TestsPlugin.Log.LogInfo(
            $"{thisItemName} second handler accepted {item.GetItemName()} from {interactor.characterName}"
        );
        interactor.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Drowsy, 0.1f);
        // do not consume item because AcceptItem_BingBong already handled it
    }

    // this is the second interact prompt, so this should not show up
    public string GetPrompt() => "ERROR";

    public bool AllowInteraction()
    {
        // only happens with healing items
        Item item = Character.localCharacter.data.currentItem;
        if (item?.canUseOnFriend ?? false)
        {
            string prompt = item
                .UIData.secondaryInteractPrompt.Replace("#targetChar", "")
                .Trim()
                .ToLower();
            return prompt.Contains("heal");
        }
        return false;
    }
}
