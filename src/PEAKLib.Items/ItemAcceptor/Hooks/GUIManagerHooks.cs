using MonoDetour;
using MonoDetour.HookGen;
using On.GUIManager;
using System.Collections.Generic;
using System.Linq;
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
            if (IItemAcceptor.TryGetItemAcceptors(self.currentInteractable, out List<IItemAcceptor> itemAcceptors))
            {
                self.interactName.SetActive(value: false);
                if (Character.localCharacter.data.currentItem && Character.localCharacter.data.currentItem.canUseOnFriend)
                {
                    self.interactPromptSecondary.SetActive(value: true);
                    self.secondaryInteractPromptText.text = itemAcceptors[0].GetSecondaryInteractionText();
                    if (itemAcceptors.All(x => x.SecondaryInteractOnly))
                    {
                        self.interactPromptPrimary.SetActive(value: false);
                    }
                }
            }
        }
    }
}
