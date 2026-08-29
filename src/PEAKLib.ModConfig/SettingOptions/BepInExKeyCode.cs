using System;
using BepInEx.Configuration;
using PEAKLib.ModConfig.SettingOptions.SettingUI;
using UnityEngine;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExKeyCode(
    ConfigEntryBase entryBase,
    string category = "Mods",
    Action<KeyCode>? saveCallback = null,
    Action<BepInExKeyCode>? onApply = null
) : BepInExInputBindingSetting<KeyCode>(entryBase, category, saveCallback)
{
    private static GameObject? _settingUICell;
    public static GameObject? SettingUICell
    {
        get
        {
            if (_settingUICell == null)
                _settingUICell = InputBindingSettingCellFactory.Create<BepInExKeyCode_SettingUI>(
                    "BepInExKeyCodeCell"
                );

            return _settingUICell;
        }
    }

    public override void ApplyValue() => onApply?.Invoke(this);

    public override GameObject? GetSettingUICell() => SettingUICell;
}
