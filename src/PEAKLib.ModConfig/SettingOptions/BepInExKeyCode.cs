using BepInEx.Configuration;
using PEAKLib.ModConfig.SettingOptions.SettingUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;
using Zorro.Settings;
using Zorro.Settings.UI;
using Object = UnityEngine.Object;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.SettingOptions;

internal class BepInExKeyCode(ConfigEntryBase entryBase, string category = "Mods",
    Action<KeyCode>? saveCallback = null,
    Action<BepInExKeyCode>? onApply = null) : Setting, IBepInExProperty, IExposedSetting
{
    ConfigEntryBase IBepInExProperty.ConfigBase { get => entryBase; }
    public KeyCode Value { get; set; }

    private static GameObject? _settingUICell = null;
    public static GameObject? SettingUICell
    {
        get
        {
            if (_settingUICell == null)
            {
                if (SingletonAsset<InputCellMapper>.Instance == null || SingletonAsset<InputCellMapper>.Instance.FloatSettingCell == null)
                    return null;

                _settingUICell = Object.Instantiate(SingletonAsset<InputCellMapper>.Instance.FloatSettingCell);
                _settingUICell.name = "BepInExKeyCodeCell";

                var oldFloatSetting = _settingUICell.GetComponent<FloatSettingUI>();
                var newKeyCodeSetting = _settingUICell.AddComponent<BepInExKeyCode_SettingUI>();

                var inputRectTransform = oldFloatSetting.inputField.GetComponent<RectTransform>();
                inputRectTransform.pivot = new Vector2(0.5f, 0.5f);
                inputRectTransform.offsetMin = new Vector2(20, -25);
                inputRectTransform.offsetMax = new Vector2(380, 25);

                newKeyCodeSetting.button = _settingUICell.AddComponent<Button>();

                oldFloatSetting.inputField.name = "Button";

                Object.DestroyImmediate(oldFloatSetting.inputField.placeholder.gameObject);
                Object.Destroy(oldFloatSetting.inputField);
                Object.DestroyImmediate(oldFloatSetting.slider.gameObject);
                Object.DestroyImmediate(oldFloatSetting);


                var text = newKeyCodeSetting.button.GetComponentInChildren<TextMeshProUGUI>();
                text.fontSize = text.fontSizeMin = text.fontSizeMax = 22;
                text.alignment = TextAlignmentOptions.Center;
                newKeyCodeSetting.text = text;

                Object.DontDestroyOnLoad(_settingUICell);
            }

            return _settingUICell;
        }
    }

    public override void Load(ISettingsSaveLoad loader) => Value = GetCurrentValue<KeyCode>(entryBase);
    public override void Save(ISettingsSaveLoad saver) => saveCallback?.Invoke(Value);
    public override void ApplyValue() => onApply?.Invoke(this);
    public override GameObject? GetSettingUICell() => SettingUICell;
    public string GetCategory() => category;
    public string GetDisplayName() => entryBase.Definition.Key;
    protected KeyCode GetDefaultValue() => GetDefaultValue<KeyCode>(entryBase);

    public override Zorro.Settings.DebugUI.SettingUI GetDebugUI(ISettingHandler settingHandler)
    {
        return null!; // only used by devs?
    }

    public void SetValue(KeyCode newValue, ISettingHandler settingHandler)
    {
        Value = newValue;
        ApplyValue();
        settingHandler.SaveSetting(this);
    }
}