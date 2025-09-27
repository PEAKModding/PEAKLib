using MonoDetour;
using MonoDetour.HookGen;
using On.PauseMenuMainPage;


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

        var settings = self.transform.parent.Find("SettingsPage"); 
        MenuAPI.settingsMenuBuilderDelegate?.Invoke(settings.gameObject.transform);
    }
}
