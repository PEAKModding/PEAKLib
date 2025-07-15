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
    /// Prompt for secondary interaction.
    /// </summary>
    /// <returns></returns>
    string GetSecondaryInteractionText();

    /// <summary>
    /// Fully consume the item, or only consume one use.
    /// </summary>
    public bool FullyConsume { get; }

    /// <summary>
    /// Suppress Primary interact menu when secondary interaction is active
    /// </summary>
    public bool SecondaryInteractOnly { get; }

    /// <summary>
    /// Check for an ItemAcceptor attached to the Interactible.
    /// </summary>
    /// <param name="interactible"></param>
    /// <returns></returns>
    public static IItemAcceptor? GetItemAcceptor(IInteractible? interactible)
    {
        MonoBehaviour? mb = interactible as MonoBehaviour;
        return mb?.GetComponent<IItemAcceptor>();
    }
}
