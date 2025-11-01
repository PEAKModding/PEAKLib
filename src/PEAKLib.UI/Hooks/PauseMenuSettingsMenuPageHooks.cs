using MonoDetour;
using MonoDetour.HookGen;
using Md.PauseMenuSettingsMenuPage;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMenuSettingsMenuPage))]
static class PauseMenuSettingsMenuPageHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(PauseMenuSettingsMenuPage self)
    {
        //need this here for now to not make breaking changes to other mods (ie. PocketPassport)
        MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.gameObject.transform);
    }
}