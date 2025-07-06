using System;
using TMPro;
using UnityEngine;
using Zorro.Settings;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using PEAKLib.Core;

namespace PEAKLib.ModConfig.SettingOptions.SettingUI;

internal class BepInExKeyCode_SettingUI : SettingInputUICell
{
    public Button? button;
    public TextMeshProUGUI? text;

    public override void Setup(Setting setting, ISettingHandler settingHandler)
    {
        ThrowHelper.ThrowIfFieldNull(button);
        ThrowHelper.ThrowIfFieldNull(text);

        if (setting == null || setting is not BepInExKeyCode keyCodeSetting) return;

        RegisterSettingListener(setting);

        text.text = keyCodeSetting.Value.ToString();

        button.onClick.AddListener(() =>
        {
            StartKeybindCapture(keyCodeSetting, settingHandler);
        });
    }

    protected override void OnSettingChangedExternal(Setting setting)
    {
        base.OnSettingChangedExternal(setting);

        if (text != null && setting is BepInExKeyCode keyCode)
            text.text = keyCode.Value.ToString();

    }

    protected override void OnDestroy()
    {
        if (detectingKey == this)
            detectingKey = null;
    }

    private static BepInExKeyCode_SettingUI? detectingKey = null;

    private static IEnumerator WaitForKey(Action<KeyCode> onKeyDetected)
    {
        // Wait for key down
        while (detectingKey != null)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    onKeyDetected?.Invoke(key);
                    yield break;
                }
            }
            yield return null;
        }
    }

    internal static KeyCode[] BlackListed = [KeyCode.Escape];
    private void StartKeybindCapture(BepInExKeyCode setting, ISettingHandler settingHandler)
    {
        if (detectingKey != null) return;

        detectingKey = this;

        if (text != null)
            text.text = "SELECT A KEY";

        StartCoroutine(WaitForKey(key =>
        {
            if (!BlackListed.Contains(key))
                setting.SetValue(key, settingHandler);

            OnSettingChangedExternal(setting);
            detectingKey = null;
        }));
    }
}
