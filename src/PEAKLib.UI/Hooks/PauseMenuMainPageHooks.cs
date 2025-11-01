using MonoDetour;
using MonoDetour.HookGen;
using Md.PauseMenuMainPage;


namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMenuMainPage))]
static class PauseMenuMainPageHooks
{
    internal static bool RunOnce = false;

    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Postfix(PostfixEnable);
    }

    private static void PostfixEnable(PauseMenuMainPage self)
    {
        MenuAPI.pauseMenuBuilderDelegate?.Invoke(self.transform);

        var controls = self.transform.parent.Find("ControlsPage");
        MenuAPI.controlsMenuBuilderDelegate?.Invoke(controls);

        // uncomment when we can get rid of PauseMenuSettingsMenuPageHooks
        // below is a breaking change for some dependent mods due to inproper usage of the settingsmenubuilderdelegate
        //var settings = self.transform.parent.Find("SettingsPage"); 
        //MenuAPI.settingsMenuBuilderDelegate?.Invoke(settings.gameObject.transform);
    }
}
