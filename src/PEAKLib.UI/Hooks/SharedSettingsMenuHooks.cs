using MonoDetour;
using MonoDetour.HookGen;
using On.SharedSettingsMenu;
using PEAKLib.UI.Elements;
using Object = UnityEngine.Object;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(SharedSettingsMenu))]
static class SharedSettingsMenuHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        RefreshSettings.Prefix(Prefix_RefreshSettings);
    }

    static void Prefix_RefreshSettings(SharedSettingsMenu self)
    {
        Templates.SettingsCellPrefab = self.m_settingsCellPrefab;

        MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.transform);
    }
}
