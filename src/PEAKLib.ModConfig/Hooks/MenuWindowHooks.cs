using MonoDetour;
using MonoDetour.HookGen;
using UnityEngine;

namespace PEAKLib.ModConfig.Hooks;

[MonoDetourTargets(typeof(MenuWindow))]
static class MenuWindowHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        On.MenuWindow.Start.Prefix(Prefix_Start);
    }

    public static GameObject? SettingsCellPrefab { get; internal set; }

    static void Prefix_Start(MenuWindow self)
    {
        if (self is not MainMenu menu || menu.settingsMenu is not PauseMainMenu settingsMenu) return;

        var sharedSettings = settingsMenu.GetComponentInChildren<SharedSettingsMenu>();

        if (sharedSettings != null)
            SettingsCellPrefab = sharedSettings.m_settingsCellPrefab;
    }
}
