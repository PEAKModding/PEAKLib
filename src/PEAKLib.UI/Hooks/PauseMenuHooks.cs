using MonoDetour;
using MonoDetour.HookGen;
using On.PauseOptionsMenu;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseOptionsMenu))]
static class PauseMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Initialize.Prefix(Prefix_Initialize);
    }

    static void Prefix_Initialize(PauseOptionsMenu self)
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
