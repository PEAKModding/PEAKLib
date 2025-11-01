﻿using PEAKLib.Items.ItemAcceptor;
using UnityEngine;

namespace PEAKLib.Tests;

/// <summary>
/// Monolith accepts items and grants some bonus stamina
/// </summary>
public class AcceptItem_Monolith : MonoBehaviour, IItemAcceptor, IInteractible
{
    public bool SecondaryInteractOnly => true;

    public void AcceptItem(Item item, Character interactor)
    {
        TestsPlugin.Log.LogInfo(
            $"Monolith accepted {item.GetItemName()} from {interactor.characterName}"
        );
        interactor.AddExtraStamina(0.125f);
        IItemAcceptor.ConsumeEntireItem(item);
    }

    public string GetPrompt()
    {
        Item item = Character.localCharacter.data.currentItem;
        if (item?.canUseOnFriend ?? false)
        {
            return $"Offer {item.GetItemName()} to Monolith";
        }
        return "";
    }

    public bool AllowInteraction() => true;

    // Typically, a separate MonoBehaviour is the IInteractible, like Item.
    // but since this is just a normal object, it cannot be interacted with normally,
    // so just return dummy values for IInteractible interface.
    public Vector3 Center() => transform.position;

    public string GetInteractionText() => "";

    public string GetName() => "Monolith";

    public Transform GetTransform() => transform;

    public void HoverEnter() { }

    public void HoverExit() { }

    public void Interact(Character interactor) { }

    public bool IsInteractible(Character interactor) =>
        Character.localCharacter.data.currentItem
        && Character.localCharacter.data.currentItem.canUseOnFriend;
}
