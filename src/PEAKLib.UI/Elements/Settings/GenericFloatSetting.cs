using System;
using Unity.Mathematics;
using Zorro.Settings;

namespace PEAKLib.UI.Elements.Settings;

/// <summary>
/// Creates a slider setting that can be added to any of the vanilla setting tabs
/// Category determines which tab to display the setting.
/// Use the saveCallback action to use the new value in your code how you see fit.
/// Can be used by other types than float, just translate your values to float on initialization and back from float on callback methods
/// </summary>
public class GenericFloatSetting(string displayName, float defaultValue = 0f, SettingsCategory category = SettingsCategory.General,
    float minValue = 0f, float maxValue = 1f, float currentValue = 0f,
    Action<float>? saveCallback = null,
    Action<GenericFloatSetting>? onApply = null) : FloatSetting, IExposedSetting
{
    /// <summary>
    /// Override for FloatSetting.Load which is used by PEAK to load the setting.
    /// </summary>
    public override void Load(ISettingsSaveLoad loader)
    {
        Value = currentValue;

        float2 minMaxValue = GetMinMaxValue();
        MinValue = minMaxValue.x;
        MaxValue = minMaxValue.y;
    }

    /// <summary>
    /// Override for FloatSetting.Save which is used by PEAK to save the setting.
    /// </summary>
    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);

    /// <summary>
    /// Override for FloatSetting.ApplyValue which is used by PEAK to apply the new value.
    /// </summary>
    public override void ApplyValue() => onApply?.Invoke(this);

    /// <summary>
    /// Override for FloatSetting.GetDisplayName which is used by PEAK to display the name of the setting.
    /// </summary>
    public string GetDisplayName() => displayName;

    /// <summary>
    /// Override for FloatSetting.GetCategory which is used by PEAK to get the category of the setting.
    /// The category is used to determine which tab should display this setting
    /// </summary>
    public string GetCategory() => category.ToString();

    /// <summary>
    /// Override for FloatSetting.GetDefaultValue which is used by PEAK to get the default value of the setting.
    /// </summary>
    protected override float GetDefaultValue() => defaultValue;

    /// <summary>
    /// Override for FloatSetting.GetMinMaxValue which is used by PEAK to get the minimum and maximum values of the setting.
    /// Min and Max values are used for the setting's slider
    /// </summary>
    protected override float2 GetMinMaxValue() => new(minValue, maxValue);
}
