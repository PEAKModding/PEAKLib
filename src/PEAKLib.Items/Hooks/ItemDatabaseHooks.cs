#if !UNITY_EDITOR
using MonoDetour;
using MonoDetour.HookGen;
using On.ItemDatabase;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(ItemDatabase))]
static class ItemDatabaseHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        OnLoaded.Prefix(Prefix_OnLoaded);
    }

    static void Prefix_OnLoaded(ItemDatabase self)
    {
        foreach (var registeredItem in ItemContent.s_RegisteredItems)
        {
            ItemRegistrar.FinishRegisterItem(self, registeredItem);
        }
        ItemRegistrar.ItemDatabaseLoaded = self;
    }
}
#endif
