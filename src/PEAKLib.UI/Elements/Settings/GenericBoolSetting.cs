using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using Zorro.Settings;

namespace PEAKLib.UI.Elements.Settings;

/// <summary>
/// Creates an on/off setting that can be added to any of the vanilla setting tabs
/// Category determines which tab to display the setting.
/// Use the saveCallback action to use the new value in your code how you see fit.
/// </summary>
public class GenericBoolSetting(
    string displayName,
    bool defaultValue = false,
    SettingsCategory category = SettingsCategory.General,
    bool currentValue = false,
    Action<bool>? saveCallback = null,
    Action<GenericBoolSetting>? onApply = null
) : OffOnSetting, IExposedSetting
{
    /// <summary>
    /// Override for OffOnSetting.Load which is used by PEAK to load the setting.
    /// </summary>
    public override void Load(ISettingsSaveLoad loader) =>
        Value = currentValue ? OffOnMode.ON : OffOnMode.OFF;

    /// <summary>
    /// Override for OffOnSetting.Save which is used by PEAK to save the setting.
    /// </summary>
    public override void Save(ISettingsSaveLoad saver) =>
        saveCallback?.Invoke(Value == OffOnMode.ON);

    /// <summary>
    /// Override for OffOnSetting.ApplyValue which is used by PEAK to apply the new value.
    /// </summary>
    public override void ApplyValue() => onApply?.Invoke(this);

    /// <summary>
    /// Override for OffOnSetting.GetDisplayName which is used by PEAK to display the name of the setting.
    /// </summary>
    protected override OffOnMode GetDefaultValue() =>
        defaultValue == true ? OffOnMode.ON : OffOnMode.OFF;

    /// <summary>
    /// Override for OffOnSetting.GetDisplayName which is used by PEAK to display the name of the setting.
    /// </summary>
    public string GetDisplayName() => displayName;

    /// <summary>
    /// Override for OffOnSetting.GetCategory which is used by PEAK to get the category of the setting.
    /// The category is used to determine which tab should display this setting
    /// </summary>
    public string GetCategory() => category.ToString();

    /// <summary>
    /// Override for EnumSetting.GetLocalizedChoices which is used by PEAK to get localized choices for this setting.
    /// Not currently doing anything.
    /// </summary>
    public override List<LocalizedString>? GetLocalizedChoices() => null!;

    /// <summary>
    /// Override for EnumSetting.GetLocalizedChoices which is used by PEAK to get localized choices for this setting.
    /// Not currently doing anything.
    /// </summary>
    public override List<string> GetUnlocalizedChoices()
    {
        return ["OFF", "ON"];
    }
}
