using System.Linq;
using Md.Item;
using MonoDetour;
using MonoDetour.HookGen;
using Photon.Pun;

namespace PEAKLib.Items.ItemAcceptor.Hooks;

[MonoDetourTargets(typeof(Item))]
internal class ItemHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        CanUseSecondary.Postfix(Postfix_CanUseSecondary);
        StartUseSecondary.Postfix(Postfix_StartUseSecondary);
        FinishCastSecondary.Postfix(Postfix_FinishCastSecondary);
    }

    static void FeedItem(IItemAcceptor itemAcceptor, Item item, Character interactor)
    {
        if (itemAcceptor != null)
        {
            itemAcceptor.AcceptItem(item, interactor);
        }
    }

    static void Postfix_CanUseSecondary(Item self, ref bool returnValue)
    {
        returnValue = returnValue || (self.canUseOnFriend && InteractionHooks.itemAcceptors.Any());
    }

    static void Postfix_StartUseSecondary(Item self)
    {
        if (!self.isUsingPrimary && !self.isUsingSecondary)
        {
            if (
                (bool)self.holderCharacter
                && self.canUseOnFriend
                && InteractionHooks.itemAcceptors.Any()
            )
            {
                // start interaction
                GameUtils.instance.StartFeed(
                    self.holderCharacter.photonView.ViewID,
                    self.holderCharacter.photonView.ViewID,
                    self.itemID,
                    self.totalSecondaryUsingTime
                );
            }
        }
    }

    static void Postfix_FinishCastSecondary(Item self)
    {
        if (self.canUseOnFriend && InteractionHooks.itemAcceptors.Any())
        {
            foreach (IItemAcceptor itemAcceptor in InteractionHooks.itemAcceptors)
            {
                FeedItem(itemAcceptor, self, self.holderCharacter);
            }
            self.photonView.RPC(
                nameof(RemoveFeedDataRPC),
                RpcTarget.All,
                self.holderCharacter.photonView.ViewID
            );
        }
    }
}
