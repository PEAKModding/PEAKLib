using System;
using Unity.Mathematics;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExFloat(string displayName, float defaultValue = 0f, string categoryName = "Mods",
    float minValue = 0f, float maxValue = 1f, float currentValue = 0f,
    Action<float>? saveCallback = null,
    Action<BepInExFloat>? onApply = null) : FloatSetting, IBepInExProperty, IExposedSetting
{
    public override void Load(ISettingsSaveLoad loader)
    {
        Value = currentValue;

        float2 minMaxValue = GetMinMaxValue();
        MinValue = minMaxValue.x;
        MaxValue = minMaxValue.y;
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);
    public override void ApplyValue() => onApply?.Invoke(this);
    public string GetDisplayName() => displayName;
    public string GetCategory() => categoryName;
    protected override float GetDefaultValue() => defaultValue;
    protected override float2 GetMinMaxValue() => new(minValue, maxValue);
}

