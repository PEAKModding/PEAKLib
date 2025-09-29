using System;
using Unity.Mathematics;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExDouble(string displayName, double defaultValue = 0f, string categoryName = "Mods",
    double minValue = 0f, double maxValue = 1f, double currentValue = 0f,
    Action<double>? saveCallback = null,
    Action<BepInExDouble>? onApply = null) : FloatSetting, IBepInExProperty, IExposedSetting
{
    public override void Load(ISettingsSaveLoad loader)
    {
        Value = Convert.ToSingle(currentValue);

        float2 minMaxValue = GetMinMaxValue();
        MinValue = minMaxValue.x;
        MaxValue = minMaxValue.y;
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);
    public override void ApplyValue() => onApply?.Invoke(this);
    public string GetDisplayName() => displayName;
    public string GetCategory() => categoryName;
    protected override float GetDefaultValue() => Convert.ToSingle(defaultValue);
    protected override float2 GetMinMaxValue() => new(Convert.ToSingle(minValue), Convert.ToSingle(maxValue));
}

