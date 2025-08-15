using MonoDetour;
using MonoDetour.HookGen;
using On.MainMenu;
using PEAKLib.Core;
using System.Linq;
using UnityEngine;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(MainMenu))]
static class PauseMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Postfix(Postfix_Start);
    }

    //Get references to our prefabs as early as possible
    static void Postfix_Start(MainMenu self)
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        MenuAPI.SettingsCellPrefab = objects.First(n => n.name == "SettingsCell");
        UIPlugin.Log.LogInfo($"SettingsCellPrefab is null - {MenuAPI.SettingsCellPrefab == null}");
        var button = objects.First(n => n.name == "UI_MainMenuButton_LeaveGame (2)");
        UIPlugin.Log.LogInfo($"button is null - {button == null}");

        if (MenuAPI.SettingsCellPrefab == null || button == null)
        {
            ThrowHelper.ThrowIfArgumentNull(button, "ButtonTemplate prefab is null! Report as issue to PEAKLib github");
            ThrowHelper.ThrowIfArgumentNull(MenuAPI.SettingsCellPrefab, "SettingsCellPrefab is null! Report as issue to PEAKLib github");
            return;
        }

        //instantiate as a new object to use as prefab because the reference we are grabbing will be destroyed at some point
        MenuAPI.ButtonTemplate = Object.Instantiate(button);
        MenuAPI.ButtonTemplate.name = "PeakUIButton";

        LocalizedText? locText =
            MenuAPI.ButtonTemplate.GetComponentInChildren<LocalizedText>();

        if (locText != null)
            Object.DestroyImmediate(locText);

        Object.DontDestroyOnLoad(MenuAPI.ButtonTemplate);
    }
}