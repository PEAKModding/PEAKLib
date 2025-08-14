using MonoDetour;
using MonoDetour.HookGen;
using On.PauseMenuMainPage;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMenuMainPage))]
static class PauseMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(PauseMenuMainPage self)
    {
        var transform = self.transform.Find("MainPage/Options");

        if (transform != null)
            MenuAPI.pauseMenuBuilderDelegate?.Invoke(transform);
        else
            UIPlugin.Log.LogError(
                "Failed to find \"MainPage/Options\" in PauseMenuHooks.Prefix_Initialize. This should not occur, please report it on PeakLib GitHub."
            );
    }
}
