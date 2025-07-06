using PEAKLib.Core;
using Zorro.UI;

namespace PEAKLib.ModConfig.Components;

internal class ModdedSettingsTABS : TABS<ModdedTABSButton>
{
    public ModdedSettingsMenu? SettingsMenu;

    public override void OnSelected(ModdedTABSButton button)
    {
        ThrowHelper.ThrowIfFieldNull(SettingsMenu);

        SettingsMenu.ShowSettings(button.category);
    }
}
