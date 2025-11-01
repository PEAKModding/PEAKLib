using System.Collections.Generic;
using System.Linq;
using Md.GUIManager;
using MonoDetour;
using MonoDetour.HookGen;
using Zorro.Core;

namespace PEAKLib.Items.ItemAcceptor.Hooks;

[MonoDetourTargets(typeof(GUIManager), Members = ["RefreshInteractablePrompt"])]
internal class GUIManagerHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        RefreshInteractablePrompt.Postfix(Postfix_RefreshInteractablePrompt);
    }

    static void Postfix_RefreshInteractablePrompt(GUIManager self)
    {
        if (self.currentInteractable.UnityObjectExists())
        {
            if (
                IItemAcceptor.TryGetItemAcceptors(
                    self.currentInteractable,
                    out IEnumerable<IItemAcceptor> itemAcceptors
                )
            )
            {
                self.interactName.SetActive(value: false);
                if (
                    Character.localCharacter.data.currentItem
                    && Character.localCharacter.data.currentItem.canUseOnFriend
                )
                {
                    self.interactPromptSecondary.SetActive(value: true);
                    self.secondaryInteractPromptText.text = itemAcceptors.First().GetPrompt();
                    if (itemAcceptors.All(x => x.SecondaryInteractOnly))
                    {
                        self.interactPromptPrimary.SetActive(value: false);
                    }
                }
            }
        }
    }
}
