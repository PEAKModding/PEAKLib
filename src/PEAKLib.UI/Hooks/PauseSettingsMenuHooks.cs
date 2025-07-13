using MonoDetour;
using MonoDetour.HookGen;
using On.PauseSettingsMenu;
using UnityEngine;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseSettingsMenu))]
static class PauseSettingsMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Initialize.Prefix(Prefix_Initialize);
    }

    static void Prefix_Initialize(PauseSettingsMenu self)
    {   
        Transform transform = self.GetComponentInChildren<SharedSettingsMenu>().transform;


        MenuAPI.settingsMenuBuilderDelegate?.Invoke(transform);
    }
}
