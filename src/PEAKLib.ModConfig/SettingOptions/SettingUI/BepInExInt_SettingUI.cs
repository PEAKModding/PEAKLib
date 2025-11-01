using TMPro;
using Zorro.Settings;

namespace PEAKLib.ModConfig.SettingOptions.SettingUI;

internal class BepInExInt_SettingUI : SettingInputUICell
{
    public TMP_InputField? inputField;

    public override void Setup(Setting setting, ISettingHandler settingHandler)
    {
        if (inputField == null || setting == null || setting is not BepInExInt intSetting)
            return;

        RegisterSettingListener(setting);
        inputField.SetTextWithoutNotify(intSetting.Expose(intSetting.Value));
        inputField.onValueChanged.AddListener(OnChanged);

        void OnChanged(string str)
        {
            if (int.TryParse(str, out var result))
            {
                inputField.SetTextWithoutNotify(intSetting.Expose(result));
                intSetting.SetValue(result, settingHandler);
            }
        }
    }

    protected override void OnSettingChangedExternal(Setting setting)
    {
        base.OnSettingChangedExternal(setting);

        if (inputField != null && setting is BepInExInt intSetting)
            inputField.SetTextWithoutNotify(intSetting.Expose(intSetting.Value));
    }
}
