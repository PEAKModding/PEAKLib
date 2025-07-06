using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Core;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExEnum(string displayName, List<string> options, string currentValue, string category = "Mods", Action<string>? saveCallback = null, Action<BepInExEnum>? onApply = null) : Setting, IEnumSetting, IExposedSetting, IBepInExProperty
{
    public string GetDisplayName() => displayName;
    public string GetCategory() => category;

    public string Value { get; protected set; } = "";

    public override void Load(ISettingsSaveLoad loader)
    {
        Value = currentValue;
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);

    public override GameObject GetSettingUICell() => SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;

    public virtual List<string> GetUnlocalizedChoices() => options;

    public List<LocalizedString> GetLocalizedChoices() => null!;

    public int GetValue()
    {
        return options.IndexOf(Value);
    }

    public void SetValue(int v, ISettingHandler settingHandler, bool fromUI)
    {
        Value = options[v];

        ApplyValue();
        settingHandler.SaveSetting(this);
        if (!fromUI) OnSettingChangedExternal();
        OnSettingChanged();
    }

    public override void ApplyValue() => onApply?.Invoke(this);

    public override Zorro.Settings.DebugUI.SettingUI GetDebugUI(ISettingHandler settingHandler) => null!;
}