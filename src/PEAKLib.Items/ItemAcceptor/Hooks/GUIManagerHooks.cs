using MonoDetour;
using MonoDetour.HookGen;
using On.GUIManager;
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
            var itemAcceptor = IItemAcceptor.GetItemAcceptor(self.currentInteractable);
            if (itemAcceptor != null)
            {
                self.interactName.SetActive(value: false);
                if (ItemHooks.HasItemCanUseOnFriend())
                {
                    self.interactPromptSecondary.SetActive(value: true);
                    self.secondaryInteractPromptText.text = itemAcceptor.GetSecondaryInteractionText();
                    if (itemAcceptor.SecondaryInteractOnly)
                    {
                        self.interactPromptPrimary.SetActive(value: false);
                    }
                }
            }
        }
    }
}
