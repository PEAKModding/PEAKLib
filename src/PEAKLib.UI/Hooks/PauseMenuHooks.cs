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
    }
}
