using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Core;
using Zorro.Settings;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExEnum(ConfigEntryBase entryBase, string category = "Mods", bool isEnum = true, Action<string>? saveCallback = null, Action<BepInExEnum>? onApply = null) : Setting, IEnumSetting, IExposedSetting, IBepInExProperty
{
    ConfigEntryBase IBepInExProperty.ConfigBase { get => entryBase; }
    public string GetDisplayName() => entryBase.Definition.Key;
    public string GetCategory() => category;

    public string Value { get; protected set; } = "";

    public void RefreshValueFromConfig()
    {
        if (isEnum)
            Value = Enum.GetName(entryBase.SettingType, entryBase.BoxedValue is object bValue ? bValue : Enum.ToObject(entryBase.SettingType, 0));
        else
            Value = GetCurrentValue<string>(entryBase);
    }

    public override void Load(ISettingsSaveLoad loader)
    {
        RefreshValueFromConfig();
    }

    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);

    public override GameObject GetSettingUICell() => SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;

    public virtual List<string> GetUnlocalizedChoices() 
    {
        if(isEnum)
            return [..Enum.GetNames(entryBase.SettingType)];
        else
        {
            return GetAcceptableValues<string>(entryBase);
        }
    } 

    public List<LocalizedString> GetLocalizedChoices() => null!;

    public int GetValue()
    {
        return GetUnlocalizedChoices().IndexOf(Value);
    }

    public void SetValue(int v, ISettingHandler settingHandler, bool fromUI)
    {
        Value = GetUnlocalizedChoices()[v];

        ApplyValue();
        settingHandler.SaveSetting(this);
        if (!fromUI) OnSettingChangedExternal();
        OnSettingChanged();
    }

    public override void ApplyValue() => onApply?.Invoke(this);

    public override Zorro.Settings.DebugUI.SettingUI GetDebugUI(ISettingHandler settingHandler) => null!;
}