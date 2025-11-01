using System;
using BepInEx.Configuration;
using Unity.Mathematics;
using Zorro.Settings;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExFloat(
    ConfigEntryBase entryBase,
    string categoryName = "Mods",
    Action<float>? saveCallback = null,
    Action<BepInExFloat>? onApply = null
) : FloatSetting, IBepInExProperty, IExposedSetting
{
    ConfigEntryBase IBepInExProperty.ConfigBase
    {
        get => entryBase;
    }

    public override void Load(ISettingsSaveLoad loader)
    {
        Value = GetCurrentValue<float>(entryBase);

        float2 minMaxValue = GetMinMaxValue();
        MinValue = minMaxValue.x;
        MaxValue = minMaxValue.y;
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);

    public override void ApplyValue() => onApply?.Invoke(this);

    public void RefreshValueFromConfig() => Value = GetCurrentValue<float>(entryBase);

    public string GetDisplayName() => entryBase.Definition.Key;

    public string GetCategory() => categoryName;

    protected override float GetDefaultValue() => GetDefaultValue<float>(entryBase);

    protected override float2 GetMinMaxValue()
    {
        if (TryGetMinMaxValue(entryBase, out float minValue, out float maxValue))
            return new(minValue, maxValue);

        return new(0f, 1000f);
    }
}
