using MonoDetour;
using MonoDetour.HookGen;
using Photon.Pun;
using On.Item;

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
            if (item != null && itemAcceptor.FullyConsume)
            {
                item.StartCoroutine(item.ConsumeDelayed(ignoreActions: true));
            }
        }
    }

    internal static bool HasItemCanUseOnFriend()
    {
        return Character.localCharacter.data.currentItem && Character.localCharacter.data.currentItem.canUseOnFriend;
    }

    static void Postfix_CanUseSecondary(Item self, ref bool returnValue)
    {
        returnValue = returnValue || (self.canUseOnFriend && InteractionHooks.itemAcceptor != null);
    }

    static void Postfix_StartUseSecondary(Item self)
    {
        if (!self.isUsingPrimary && !self.isUsingSecondary)
        {
            if ((bool)self.holderCharacter && self.canUseOnFriend && InteractionHooks.itemAcceptor != null)
            {
                // start interaction
                GameUtils.instance.StartFeed(self.holderCharacter.photonView.ViewID, self.holderCharacter.photonView.ViewID, self.itemID, self.totalSecondaryUsingTime);
            }
        }
    }

    static void Postfix_FinishCastSecondary(Item self)
    {
        if (self.canUseOnFriend && InteractionHooks.itemAcceptor != null)
        {
            FeedItem(InteractionHooks.itemAcceptor, self, self.holderCharacter);
            self.photonView.RPC("RemoveFeedDataRPC", RpcTarget.All, self.holderCharacter.photonView.ViewID);
        }
    }
}
