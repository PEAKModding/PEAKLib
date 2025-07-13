using MonoDetour;
using MonoDetour.HookGen;
using On.PauseMainMenu;
using UnityEngine;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMainMenu))]
static class PauseMainMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Initialize.Prefix(Prefix_Initialize);
    }

    static void Prefix_Initialize(PauseMainMenu self)
    {   
        Transform transform = self.GetComponentInChildren<SharedSettingsMenu>().transform;


        MenuAPI.settingsMenuBuilderDelegate?.Invoke(transform);
    }
}
