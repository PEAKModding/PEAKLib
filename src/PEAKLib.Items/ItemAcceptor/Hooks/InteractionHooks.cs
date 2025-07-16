using MonoDetour;
using MonoDetour.HookGen;
using On.Interaction;
using System.Collections.Generic;

namespace PEAKLib.Items.ItemAcceptor.Hooks;

[MonoDetourTargets(typeof(Interaction))]
internal class InteractionHooks
{
    internal static IEnumerable<IItemAcceptor> itemAcceptors = [];

    [MonoDetourHookInitialize]
    static void Init()
    {
        LateUpdate.Postfix(Postfix_LateUpdate);
    }

    static void Postfix_LateUpdate(Interaction self)
    {
        IItemAcceptor.TryGetItemAcceptors(self.bestInteractable, out itemAcceptors);
    }
}
