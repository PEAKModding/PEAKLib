﻿using PEAKLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zorro.Core;
using Zorro.Settings;
using Zorro.Settings.UI;

namespace PEAKLib.UI.Elements;

// WIP
/// <summary>
/// Used to create TextInput
/// </summary>
public class PeakTextInput : PeakElement
{
    private static GameObject? _textInputPrefab;
    internal static GameObject TextInputPrefab
    {
        get
        {
            if (_textInputPrefab == null)
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

                _textInputPrefab = Instantiate(
                    SingletonAsset<InputCellMapper>.Instance.FloatSettingCell
                );
                _textInputPrefab.name = "PeakTextInput";

                if (_textInputPrefab.transform is RectTransform rectTransform)
                {
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(200, 80);
                }

                var oldFloatSetting = _textInputPrefab.GetComponent<FloatSettingUI>();
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

                DontDestroyOnLoad(_textInputPrefab);
            }

            return _textInputPrefab;
        }
    }

    internal static PeakTextInput Create() =>
        Instantiate(TextInputPrefab).AddComponent<PeakTextInput>();

    /// <summary>
    /// TMP_InputField component
    /// </summary>
    public TMP_InputField InputField { get; private set; } = null!;

    private TextMeshProUGUI Placeholder { get; set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        InputField = GetComponentInChildren<TMP_InputField>();

        Placeholder = InputField.placeholder.GetComponent<TextMeshProUGUI>();
        Placeholder.text = "";
    }

    /// <summary>
    /// Same as InputField.onValueChanged.AddListener()
    /// </summary>
    /// <param name="onValueChanged"></param>
    /// <returns></returns>
    public PeakTextInput OnValueChanged(UnityAction<string> onValueChanged)
    {
        ThrowHelper.ThrowIfArgumentNull(onValueChanged);

        InputField.onValueChanged.AddListener(onValueChanged);

        return this;
    }

    /// <summary>
    /// A text for when input is empty
    /// </summary>
    /// <param name="placeholder"></param>
    /// <returns></returns>
    public PeakTextInput SetPlaceholder(string placeholder)
    {
        Placeholder.text = placeholder;

        return this;
    }
}
