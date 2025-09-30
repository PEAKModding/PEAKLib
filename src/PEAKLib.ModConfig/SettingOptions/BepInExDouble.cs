using BepInEx.Configuration;
using System;
using Unity.Mathematics;
using Zorro.Settings;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExDouble(ConfigEntryBase entryBase, string categoryName = "Mods",
    Action<double>? saveCallback = null,
    Action<BepInExDouble>? onApply = null) : FloatSetting, IBepInExProperty, IExposedSetting
{
    ConfigEntryBase IBepInExProperty.ConfigBase { get => entryBase; }

    public override void Load(ISettingsSaveLoad loader)
    {
        Value = Convert.ToSingle(GetCurrentValue<double>(entryBase));

        float2 minMaxValue = GetMinMaxValue();
        MinValue = minMaxValue.x;
        MaxValue = minMaxValue.y;
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);
    public override void ApplyValue() => onApply?.Invoke(this);
    public void RefreshValueFromConfig() => Value = Convert.ToSingle(GetCurrentValue<double>(entryBase));
    public string GetDisplayName() => entryBase.Definition.Key;
    public string GetCategory() => categoryName;
    protected override float GetDefaultValue()
    {
        double def = GetDefaultValue<double>(entryBase);
        return Convert.ToSingle(def);
    }
    protected override float2 GetMinMaxValue()
    {
        if(TryGetMinMaxValue(entryBase, out double minValue, out double maxValue))
        {
            float min = Convert.ToSingle(minValue);
            float max = Convert.ToSingle(maxValue);

            return new(min, max);
        }

        return new(0f, 1000f);
    }
}

