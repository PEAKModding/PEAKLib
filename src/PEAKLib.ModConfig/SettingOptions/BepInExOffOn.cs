using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine.Localization;
using Zorro.Settings;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExOffOn(
    ConfigEntryBase entryBase,
    string category = "Mods",
    Action<bool>? saveCallback = null,
    Action<BepInExOffOn>? onApply = null
) : OffOnSetting, IBepInExProperty, IExposedSetting
{
    ConfigEntryBase IBepInExProperty.ConfigBase
    {
        get => entryBase;
    }

    public override void Load(ISettingsSaveLoad loader) =>
        Value = GetCurrentValue<bool>(entryBase) ? OffOnMode.ON : OffOnMode.OFF;

    public override void Save(ISettingsSaveLoad saver) =>
        saveCallback?.Invoke(Value == OffOnMode.ON);

    public void RefreshValueFromConfig() =>
        Value = GetCurrentValue<bool>(entryBase) ? OffOnMode.ON : OffOnMode.OFF;

    public override void ApplyValue() => onApply?.Invoke(this);

    protected override OffOnMode GetDefaultValue() =>
        GetDefaultValue<bool>(entryBase) == true ? OffOnMode.ON : OffOnMode.OFF;

    public string GetDisplayName() => entryBase.Definition.Key;

    public string GetCategory() => category;

    public override List<LocalizedString>? GetLocalizedChoices() => null;
}
