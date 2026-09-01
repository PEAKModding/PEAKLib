using PEAKLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;
using Zorro.Settings;
using Zorro.Settings.UI;
using Object = UnityEngine.Object;

namespace PEAKLib.ModConfig.SettingOptions.SettingUI;

internal abstract class InputBindingSettingUI : SettingInputUICell
{
    public Button? button;
    public TextMeshProUGUI? text;

    protected void SetupBinding(Setting setting)
    {
        ThrowHelper.ThrowIfFieldNull(button);
        ThrowHelper.ThrowIfFieldNull(text);
        RegisterSettingListener(setting);
    }

    protected void ShowCapturePrompt() => text!.text = "SELECT A KEY";

    protected virtual void OnDisable() => CancelCapture();

    protected override void OnDestroy()
    {
        CancelCapture();
        base.OnDestroy();
    }

    private void CancelCapture()
    {
        if (
            ModConfigPlugin.instance != null
            && ModConfigPlugin.instance.InputBindingCapture != null
        )
            ModConfigPlugin.instance.InputBindingCapture.Cancel(this);
    }
}

internal static class InputBindingSettingCellFactory
{
    public static GameObject? Create<TSettingUI>(string name)
        where TSettingUI : InputBindingSettingUI
    {
        InputCellMapper? mapper = SingletonAsset<InputCellMapper>.Instance;
        if (mapper == null || mapper.FloatSettingCell == null)
            return null;

        GameObject cell = Object.Instantiate(mapper.FloatSettingCell);
        cell.name = name;

        FloatSettingUI? oldFloatSetting = cell.GetComponent<FloatSettingUI>();
        if (oldFloatSetting == null)
        {
            ModConfigPlugin.Log.LogError("FloatSettingCell is missing FloatSettingUI.");
            Object.Destroy(cell);
            return null;
        }

        TSettingUI settingUI = cell.AddComponent<TSettingUI>();
        RectTransform inputRectTransform = oldFloatSetting.inputField.GetComponent<RectTransform>();
        inputRectTransform.pivot = new Vector2(0.5f, 0.5f);
        inputRectTransform.offsetMin = new Vector2(20, -25);
        inputRectTransform.offsetMax = new Vector2(380, 25);

        settingUI.button = cell.AddComponent<Button>();
        oldFloatSetting.inputField.name = "Button";
        Object.DestroyImmediate(oldFloatSetting.inputField.placeholder.gameObject);
        Object.Destroy(oldFloatSetting.inputField);
        Object.DestroyImmediate(oldFloatSetting.slider.gameObject);
        Object.DestroyImmediate(oldFloatSetting);

        TextMeshProUGUI text = settingUI.button.GetComponentInChildren<TextMeshProUGUI>();
        text.enableAutoSizing = true;
        text.fontSize = text.fontSizeMax = 28;
        text.fontSizeMin = 18;
        text.alignment = TextAlignmentOptions.Midline;
        settingUI.text = text;

        Object.DontDestroyOnLoad(cell);
        return cell;
    }
}
