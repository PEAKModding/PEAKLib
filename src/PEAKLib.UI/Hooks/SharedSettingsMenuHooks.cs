/*using MonoDetour;
using MonoDetour.HookGen;
using On.SharedSettingsMenu;
using PEAKLib.UI.Elements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(SharedSettingsMenu))]
static class SharedSettingsMenuHook
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        OnEnable.Prefix(Prefix_OnEnable);
    }

    static void Prefix_OnEnable(SharedSettingsMenu self)
    {
        var main = self.gameObject.GetComponentInParent<MainMenuSettingsPage>();
        var pause = self.gameObject.GetComponentInParent<PauseMainMenu>();
        
        //this settings menu is in the main menu
        if(main != null)
        {
            MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.transform);

            var mainPage = GameObject.Find("MainMenu/Canvas/MainPage");
            if (mainPage != null)
                MenuAPI.mainMenuBuilderDelegate?.Invoke(mainPage.GetComponent<MainMenuMainPage>().m_playButton.transform.parent);
        }

        //this settings menu is in the pause menu
        if(pause != null)
        {
            MenuAPI.pauseMenuBuilderDelegate?.Invoke(pause.backButton.transform);
            MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.transform);
        }
    }
}*/