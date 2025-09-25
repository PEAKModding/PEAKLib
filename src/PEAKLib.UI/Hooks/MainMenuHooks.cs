using MonoDetour;
using MonoDetour.HookGen;
using On.MainMenu;
using PEAKLib.Core;
using PEAKLib.UI.Elements;
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
        Templates.SettingsCellPrefab = objects.First(n => n.name == "SettingsCell");
        UIPlugin.Log.LogDebug($"SettingsCellPrefab is null - {Templates.SettingsCellPrefab == null}");
        var button = objects.First(n => n.name == "UI_MainMenuButton_LeaveGame (2)");
        UIPlugin.Log.LogDebug($"button is null - {button == null}");

        if (Templates.SettingsCellPrefab == null || button == null)
        {
            ThrowHelper.ThrowIfArgumentNull(button, "ButtonTemplate prefab is null! Report as issue to PEAKLib github");
            ThrowHelper.ThrowIfArgumentNull(Templates.SettingsCellPrefab, "SettingsCellPrefab is null! Report as issue to PEAKLib github");
            return;
        }

        //instantiate as a new object to use as prefab because the reference we are grabbing will be destroyed at some point
        Templates.ButtonTemplate = Object.Instantiate(button)!;
        Templates.ButtonTemplate.name = "PeakUIButton";

        LocalizedText? locText =
            Templates.ButtonTemplate.GetComponentInChildren<LocalizedText>();

        if (locText != null)
            Object.DestroyImmediate(locText);

        Object.DontDestroyOnLoad(Templates.ButtonTemplate);
    }
}