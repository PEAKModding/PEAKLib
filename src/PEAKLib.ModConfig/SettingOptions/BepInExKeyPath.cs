using System;
using BepInEx.Configuration;
using PEAKLib.ModConfig.SettingOptions.SettingUI;
using UnityEngine;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExKeyPath(
    ConfigEntryBase entryBase,
    string category = "Mods",
    Action<string>? saveCallback = null,
    Action<BepInExKeyPath>? onApply = null
) : BepInExInputBindingSetting<string>(entryBase, category, saveCallback)
{
    private static GameObject? _settingUICell;
    public static GameObject? SettingUICell
    {
        get
        {
            if (_settingUICell == null)
                _settingUICell = InputBindingSettingCellFactory.Create<BepInExKeyPath_SettingUI>(
                    "BepInExKeyPathCell"
                );

            return _settingUICell;
        }
    }

    public override void ApplyValue() => onApply?.Invoke(this);

    public override GameObject? GetSettingUICell() => SettingUICell;
}
