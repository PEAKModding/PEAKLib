using MonoDetour;
using MonoDetour.HookGen;
using On.SharedSettingsMenu;
using PEAKLib.UI.Elements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(SharedSettingsMenu))]
static class SharedSettingsMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        OnEnable.Prefix(Prefix_OnEnable);
    }

    static void Prefix_OnEnable(SharedSettingsMenu self)
    {   
        Transform transform = self.transform;


        MenuAPI.settingsMenuBuilderDelegate?.Invoke(transform);
    }
}
