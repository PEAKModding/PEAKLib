using System;
using BepInEx.Configuration;
using UnityEngine;
using Zorro.Settings;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal abstract class BepInExInputBindingSetting<TValue>(
    ConfigEntryBase entryBase,
    string category,
    Action<TValue>? saveCallback
) : Setting, IBepInExProperty, IExposedSetting
{
    ConfigEntryBase IBepInExProperty.ConfigBase => entryBase;

    public TValue Value { get; private set; } = GetCurrentValue<TValue>(entryBase);

    public override void Load(ISettingsSaveLoad loader) => RefreshValueFromConfig();

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);

    public void RefreshValueFromConfig() => Value = GetCurrentValue<TValue>(entryBase);

    public string GetCategory() => category;

    public string GetDisplayName() => entryBase.Definition.Key;

    public override Zorro.Settings.DebugUI.SettingUI GetDebugUI(ISettingHandler settingHandler) =>
        null!;

    public void SetValue(TValue newValue, ISettingHandler settingHandler)
    {
        Value = newValue;
        ApplyValue();
        settingHandler.SaveSetting(this);
    }
}
