/*using MonoDetour;
using MonoDetour.HookGen;
using On.MenuWindow;
using PEAKLib.UI.Elements;
using Object = UnityEngine.Object;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(MenuWindow))]
static class MenuWindowHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(MenuWindow self)
    {
        if (self is PauseMainMenu mainSettingMenu)
        {
            var sharedSettingsMenu = mainSettingMenu.GetComponentInChildren<SharedSettingsMenu>();
            if (sharedSettingsMenu != null)
                MenuAPI.settingsMenuBuilderDelegate?.Invoke(sharedSettingsMenu.transform);
        }
        else if (self is PauseSettingsMenu pauseSettingsMenu)
        {
            var sharedSettingsMenu = pauseSettingsMenu.GetComponentInChildren<SharedSettingsMenu>();
            if (sharedSettingsMenu != null)
                MenuAPI.settingsMenuBuilderDelegate?.Invoke(sharedSettingsMenu.transform);
        }


        if (self is not MainMenu menu || menu.settingsMenu is not PauseMainMenu settingsMenu)
            return;

        var sharedSettings = settingsMenu.GetComponentInChildren<SharedSettingsMenu>();

        if (sharedSettings != null)
            Templates.SettingsCellPrefab = sharedSettings.m_settingsCellPrefab;
        else
            UIPlugin.Log.LogError(
                "Failed to find SharedSettingsMenu in MenuWindowHooks.Prefix_Start. This should not occur, please report it on PeakLib GitHub."
            );

        if (Templates.ButtonTemplate == null)
        {
            Templates.ButtonTemplate = Object.Instantiate(settingsMenu.backButton.gameObject);
            Templates.ButtonTemplate.name = "PeakUIButton";

            LocalizedText? locText =
                Templates.ButtonTemplate.GetComponentInChildren<LocalizedText>();

            if (locText != null)
                Object.DestroyImmediate(locText);

            Object.DontDestroyOnLoad(Templates.ButtonTemplate);
        }

        MenuAPI.mainMenuBuilderDelegate?.Invoke(menu.playWithFriendsButton.transform.parent);
    }
}
*/