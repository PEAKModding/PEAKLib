using System.Collections.Generic;
using Md.Interaction;
using MonoDetour;
using MonoDetour.HookGen;

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
