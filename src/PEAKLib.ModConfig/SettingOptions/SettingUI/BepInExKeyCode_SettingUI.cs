using PEAKLib.ModConfig;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions.SettingUI;

internal class BepInExKeyCode_SettingUI : InputBindingSettingUI
{
    public override void Setup(Setting setting, ISettingHandler settingHandler)
    {
        if (setting is not BepInExKeyCode keyCodeSetting)
            return;

        SetupBinding(setting);
        InputBindingDisplay.SetText(text!, keyCodeSetting.Value);
        button!.onClick.AddListener(() => StartKeybindCapture(keyCodeSetting, settingHandler));
    }

    protected override void OnSettingChangedExternal(Setting setting)
    {
        base.OnSettingChangedExternal(setting);

        if (text != null && setting is BepInExKeyCode keyCode)
            InputBindingDisplay.SetText(text, keyCode.Value);
    }

    private void StartKeybindCapture(BepInExKeyCode setting, ISettingHandler settingHandler)
    {
        bool started = ModConfigPlugin.instance.InputBindingCapture.TryCaptureKeyCode(
            this,
            keyCode =>
            {
                try
                {
                    setting.SetValue(keyCode, settingHandler);
                }
                finally
                {
                    OnSettingChangedExternal(setting);
                }
            },
            () => OnSettingChangedExternal(setting)
        );

        if (started)
            ShowCapturePrompt();
    }
}
