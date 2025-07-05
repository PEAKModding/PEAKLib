using TMPro;
using UnityEngine;
using Zorro.Core;
using Zorro.Settings;
using Zorro.Settings.UI;

namespace PEAKLib.UI.Elements;

// WIP
public class PeakTextInput : PeakElement
{
    private static GameObject? _textInput;
    public static GameObject TextInput
    {
        get
        {
            if (_textInput == null)
            {
                if (
                    SingletonAsset<InputCellMapper>.Instance == null
                    || SingletonAsset<InputCellMapper>.Instance.FloatSettingCell == null
                )
                {
                    throw new System.Exception(
                        "Tried to create a TextInput but prefab was not found. Please wait until the game fully loads to create UI elements."
                    );
                }

                _textInput = Instantiate(SingletonAsset<InputCellMapper>.Instance.FloatSettingCell);
                _textInput.name = "PeakTextInput";

                var oldFloatSetting = _textInput.GetComponent<FloatSettingUI>();
                var inputField = oldFloatSetting.inputField;

                DestroyImmediate(oldFloatSetting.slider.gameObject);
                DestroyImmediate(oldFloatSetting);

                inputField.characterValidation = TMP_InputField.CharacterValidation.None;
                var inputRectTransform = inputField.GetComponent<RectTransform>();

                inputRectTransform.pivot = new Vector2(0.5f, 0.5f);
                Utilities.ExpandToParent(inputRectTransform);

                var texts = inputField.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    text.fontSize = text.fontSizeMin = text.fontSizeMax = 22;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                }

                DontDestroyOnLoad(_textInput);
            }

            return _textInput;
        }
    }

    internal static PeakTextInput Create() => Instantiate(TextInput).AddComponent<PeakTextInput>();

    private void Awake() { }
}
