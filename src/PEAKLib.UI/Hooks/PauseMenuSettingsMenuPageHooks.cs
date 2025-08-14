using MonoDetour;
using MonoDetour.HookGen;
using System.Linq;
using UnityEngine;
using On.PauseMenuSettingsMenuPage;
using PEAKLib.Core;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMenuSettingsMenuPage))]
static class PauseMenuSettingsMenuPageHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(PauseMenuSettingsMenuPage self)
    {
        MenuAPI.pauseMenuBuilderDelegate?.Invoke(self.backButton.transform);
        MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.gameObject.transform);
    }
}