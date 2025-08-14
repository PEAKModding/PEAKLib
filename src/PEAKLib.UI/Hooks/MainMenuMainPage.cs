using MonoDetour;
using MonoDetour.HookGen;
using On.MainMenuMainPage;
using PEAKLib.UI.Elements;
using Object = UnityEngine.Object;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(MainMenuMainPage))]
static class MainMenuMainPageHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(MainMenuMainPage self)
    {
        if (Templates.ButtonTemplate == null)
        {
            var settingsMenu = self
                .transform.parent.Find("SettingsPage")
                .GetComponent<MainMenuSettingsPage>();

            Templates.ButtonTemplate = Object.Instantiate(settingsMenu.backButton.gameObject);
            Templates.ButtonTemplate.name = "PeakUIButton";

            LocalizedText? locText =
                Templates.ButtonTemplate.GetComponentInChildren<LocalizedText>();

            if (locText != null)
                Object.DestroyImmediate(locText);

            Object.DontDestroyOnLoad(Templates.ButtonTemplate);
        }

        MenuAPI.mainMenuBuilderDelegate?.Invoke(self.m_playButton.transform.parent);
    }
}
