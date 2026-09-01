using PEAKLib.ModConfig;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions.SettingUI;

internal class BepInExKeyPath_SettingUI : InputBindingSettingUI
{
    public override void Setup(Setting setting, ISettingHandler settingHandler)
    {
        if (setting is not BepInExKeyPath keyPathSetting)
            return;

        SetupBinding(setting);
        InputBindingDisplay.SetText(text!, keyPathSetting.Value);
        button!.onClick.AddListener(() => StartKeybindCapture(keyPathSetting, settingHandler));
    }

    protected override void OnSettingChangedExternal(Setting setting)
    {
        base.OnSettingChangedExternal(setting);

        if (text != null && setting is BepInExKeyPath keyPathSetting)
            InputBindingDisplay.SetText(text, keyPathSetting.Value);
    }

    private void StartKeybindCapture(BepInExKeyPath setting, ISettingHandler settingHandler)
    {
        bool started = ModConfigPlugin.instance.InputBindingCapture.TryCapturePath(
            this,
            path =>
            {
                try
                {
                    setting.SetValue(path, settingHandler);
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
