using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExOffOn(string displayName, bool defaultValue = false, string category = "Mods", bool currentValue = false, Action<bool>? saveCallback = null, Action<BepInExOffOn>? onApply = null) : OffOnSetting, IBepInExProperty, IExposedSetting
{
    public override void Load(ISettingsSaveLoad loader) => Value = currentValue ? OffOnMode.ON : OffOnMode.OFF;
    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value == OffOnMode.ON);
    public override void ApplyValue() => onApply?.Invoke(this);
    protected override OffOnMode GetDefaultValue() => defaultValue == true ? OffOnMode.ON : OffOnMode.OFF;
    public string GetDisplayName() => displayName;
    public string GetCategory() => category;
    public override List<LocalizedString> GetLocalizedChoices() => null;
}
