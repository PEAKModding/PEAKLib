using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Core;
using Zorro.Settings;

namespace PEAKLib.UI.Elements.Settings;

/// <summary>
/// Creates an enum type setting that can be added to any of the vanilla setting tabs
/// Category determines which tab to display the setting.
/// Use the saveCallback action to use the new value in your code how you see fit.
/// </summary>
public class GenericEnumSetting<T>(string displayName, T currentValue, T defaultValue, SettingsCategory category = SettingsCategory.General, Action<T>? saveCallback = null, Action<GenericEnumSetting<T>>? onApply = null) : EnumSetting<T>, IExposedSetting where T : unmanaged, Enum
{
    /// <summary>
    /// Override for OffOnSetting.Load which is used by PEAK to load the setting.
    /// </summary>
    public override void Load(ISettingsSaveLoad loader) => Value = currentValue;

    /// <summary>
    /// Override for Save which is used by PEAK to save the setting.
    /// </summary>
    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);
    /// <summary>
    /// Override for GetSettingUICell which is used by PEAK as a template to create the setting type.
    /// </summary>
    public override GameObject GetSettingUICell() => SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;
    /// <summary>
    /// Override for ApplyValue which is used by PEAK to apply the new value.
    /// </summary>
    public override void ApplyValue() => onApply?.Invoke(this);
    /// <summary>
    /// Override for GetDisplayName which is used by PEAK to display the name of the setting.
    /// </summary>
    protected override T GetDefaultValue() => defaultValue;
    /// <summary>
    /// Override for GetDisplayName which is used by PEAK to display the name of the setting.
    /// </summary>
    public string GetDisplayName() => displayName;
    /// <summary>
    /// Override for GetCategory which is used by PEAK to get the category of the setting.
    /// The category is used to determine which tab should display this setting
    /// </summary>
    public string GetCategory() => category.ToString();
    /// <summary>
    /// Override for GetLocalizedChoices which is used by PEAK to get localized choices for this setting.
    /// Not currently doing anything.
    /// </summary>
    public override List<LocalizedString>? GetLocalizedChoices() => null!;

    /// <summary>
    /// Override for GetUnlocalizedChoices which is used by PEAK to display the unlocalized choices for this setting.
    /// This is the string representation of your enum's possible values
    /// </summary>
    public override List<string> GetUnlocalizedChoices() => [.. Enum.GetNames(typeof(T))];

    /// <summary>
    /// Override for GetDebugUI which is used by PEAK to create a debug setting cell
    /// Set to null so that no debug setting is made
    /// </summary>
    public override Zorro.Settings.DebugUI.SettingUI GetDebugUI(ISettingHandler settingHandler) => null!;
}
