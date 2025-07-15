using MonoDetour;
using MonoDetour.HookGen;
using On.Interaction;

namespace PEAKLib.Items.ItemAcceptor.Hooks;

[MonoDetourTargets(typeof(Interaction))]
internal class InteractionHooks
{
    internal static IItemAcceptor? itemAcceptor = null;

    [MonoDetourHookInitialize]
    static void Init()
    {
        LateUpdate.Postfix(Postfix_LateUpdate);
    }

    static void Postfix_LateUpdate(Interaction self)
    {
        itemAcceptor = IItemAcceptor.GetItemAcceptor(self.bestInteractable);
    }
}
