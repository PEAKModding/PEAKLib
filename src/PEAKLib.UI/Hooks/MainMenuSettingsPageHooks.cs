using MonoDetour;
using MonoDetour.HookGen;
using System.Linq;
using UnityEngine;
using On.MainMenuSettingsPage;
using PEAKLib.Core;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(MainMenuSettingsPage))]
static class MainMenuSettingsPageHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(MainMenuSettingsPage self)
    {
        MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.transform);

        var selector = self.GetComponentInParent<MainMenuPageSelector>();
        if (selector != null)
            MenuAPI.mainMenuBuilderDelegate?.Invoke(selector.mainPage.m_playButton.transform.parent);
    }
}