using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PEAKLib.Items.ItemAcceptor;

/// <summary>
/// Allows players to feed/use items on this IInteractible.
/// </summary>
public interface IItemAcceptor
{
    /// <summary>
    /// Called when a player uses an item on the GameObject with an IItemAcceptor component.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="interactor"></param>
    void AcceptItem(Item item, Character interactor);

    /// <summary>
    /// Check whether a secondary interaction is valid.
    /// </summary>
    /// <returns></returns>
    bool AllowInteraction();

    /// <summary>
    /// Prompt for secondary interaction.
    /// </summary>
    /// <returns></returns>
    string GetPrompt();

    /// <summary>
    /// Suppress Primary interact menu when secondary interaction is active
    /// </summary>
    bool SecondaryInteractOnly { get; }

    /// <summary>
    /// Check for ItemAcceptors attached to the Interactible.
    /// </summary>
    /// <param name="interactible"></param>
    /// <param name="itemAcceptors"></param>
    /// <returns></returns>
    public static bool TryGetItemAcceptors(
        IInteractible? interactible,
        out IEnumerable<IItemAcceptor> itemAcceptors
    )
    {
        MonoBehaviour? mb = interactible as MonoBehaviour;
        if (mb != null)
        {
            itemAcceptors = mb.GetComponents<IItemAcceptor>().Where(x => x.AllowInteraction());
        }
        else
        {
            itemAcceptors = [];
        }
        return itemAcceptors.Any();
    }

    /// <summary>
    /// Consume one use from the item.
    /// Item is removed if it is used up and marked to be consumed on full use.
    /// </summary>
    public static void ConsumeOneUse(Item item)
    {
        ConsumeUses(item, 1);
    }

    /// <summary>
    /// Consume all uses from the item.
    /// Item is removed if it is used up and marked to be consumed on full use.
    /// </summary>
    public static void ConsumeAllUses(Item item)
    {
        ConsumeUses(item, item.totalUses);
    }

    /// <summary>
    /// Item is removed, even if it is NOT consumed on full use.
    /// </summary>
    public static void ConsumeEntireItem(Item item)
    {
        item.StartCoroutine(item.ConsumeDelayed(ignoreActions: true));
    }

    internal static void ConsumeUses(Item item, int uses)
    {
        Action_ReduceUses reduceUses = item.GetComponent<Action_ReduceUses>();
        if (reduceUses == null)
        {
            bool multiUse = item.totalUses > 1;
            if (!multiUse)
            {
                ConsumeEntireItem(item);
                return;
            }
            ItemsPlugin.Log.LogWarning(
                $"{item} is multi-use but lacks the Action_ReduceUses component."
            );
            return;
        }
        for (int i = 0; i < uses; i++)
        {
            reduceUses.RunAction();
        }
    }
}
