using BepInEx.Configuration;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PEAKLib.ModConfig.SettingsHandlerUtility;

namespace PEAKLib.ModConfig.Components;

internal class ModdedRebindKey : PeakElement
{
    public string inputActionName = string.Empty;
    internal GameObject warning = null!;
    internal GameObject rebind = null!;
    internal GameObject reset = null!;

    internal Color defaultTextColor = new(0.8745f, 0.8549f, 0.7608f);
    internal Color overriddenTextColor = new(0.8679f, 0.7459f, 0.3316f);

    internal ConfigEntry<KeyCode>? ConfigKeyCode { get; set; } = null!;
    internal ConfigEntry<string>? ConfigKeyString { get; set; } = null!;
    internal string ConfigName { get; set; } = string.Empty;
    private PeakButton button = null!;
    internal PeakText value = null!;
    internal GameObject Label = null!;

    //this shit was a LOT of trial and error to get things looking better in a uniform way
    internal void Setup(ConfigEntryBase entry, string ModName)
    {
        if (entry is ConfigEntry<KeyCode> keyCodeEntry)
            ConfigKeyCode = keyCodeEntry;

        if (entry is ConfigEntry<string> keyString)
            ConfigKeyString = keyString;

        ConfigName = entry.Definition.Key;

        inputActionName = $"({ModName}) {ConfigName}";

        button = MenuAPI
            .CreateButton(inputActionName)
            .ParentTo(transform)
            .SetSize(new(1025f, 60f))
            .OnClick(RebindOperation);

        button.Button.targetGraphic.color = new(0.3396f, 0.204f, 0.1362f, 0.7294f); //match original controls page
        button.Text.TextMesh.alignment = TextAlignmentOptions.TopLeft;
        button.Text.TextMesh.margin = new(15f, 5f, 0, 0);
        button.Text.TextMesh.color = defaultTextColor;
        button.Text.TextMesh.enableAutoSizing = true;
        button.Text.TextMesh.fontSizeMin = 18f;
        button.Text.TextMesh.fontSizeMax = 30f;

        //this game object holds the reset button and the value (the right side of the control setting)
        GameObject rightParent = new($"{ModName} Value");
        rightParent.transform.SetParent(transform);
        rightParent.AddComponent<LayoutElement>();
        rightParent.GetComponent<RectTransform>().anchoredPosition = new(500f, 0f);

        string valueText = GetInitialValue(entry);

        value = MenuAPI
            .CreateText(valueText)
            .ParentTo(rightParent.transform)
            .SetPosition(new(0f, -20f))
            .SetFontSize(34f)
            .SetColor(defaultTextColor);

        //Below is the setup necessary for the text to behave in a way that it does
        //not overlap the reset button and will instead position itself further left
        var layout = value.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        var layoutelement = value.gameObject.AddComponent<LayoutElement>();
        layoutelement.preferredWidth = 60f;
        var content = value.gameObject.AddComponent<ContentSizeFitter>();
        content.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        value.TextMesh.horizontalAlignment = HorizontalAlignmentOptions.Right; //IMPORTANT
        value.TextMesh.textWrappingMode = TextWrappingModes.NoWrap; //IMPORTANT

        //This translates the sprite tag to the corresponding sprite
        value.gameObject.AddComponent<TMP_SpriteAnimator>();

        //This is our warning icon that shows when a control shares the same binding as another
        warning = GameObject.Instantiate(Templates.BindWarningButton!, transform);
        warning.transform.localPosition = new(-535f, 0f, 0f);
        warning.SetActive(false);

        //Reset to default button
        reset = GameObject.Instantiate(Templates.ResetBindButton!, rightParent.transform);
        reset.GetComponent<Button>().onClick.RemoveAllListeners();
        reset.GetComponent<Button>().onClick.AddListener(ResetThis);

        //Reset button needs the below so it maintains a consistent size
        var layoutElement = reset.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 32f;
        layoutElement.preferredHeight = 32f;
        layoutElement.flexibleHeight = 0f;
        layoutElement.flexibleWidth = 0f;

        //Below ensures reset button is always at the same position of the setting horizontally
        var rect = reset.GetComponent<RectTransform>();
        rect.anchorMin = new(1f, 0.5f);
        rect.anchorMax = new(1f, 0.5f);
        rect.pivot = new(1f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    private static string GetInitialValue(ConfigEntryBase entry)
    {
        if (entry is ConfigEntry<KeyCode> keyCodeValue)
            return keyCodeValue.Value.ToString();

        if (entry is ConfigEntry<string> stringValue)
            return stringValue.Value;

        ModConfigPlugin.Log.LogWarning(
            "Unexpected config entry type being provided to Modded Controls Menu!"
        );

        return string.Empty;
    }

    //Reset button action, resets the setting to the default value
    private void ResetThis()
    {
        //var current = InputHandler.GetCurrentUsedInputScheme();

        if (ConfigKeyCode != null)
            ConfigKeyCode.Value = GetDefaultValue(ConfigKeyCode);

        if (ConfigKeyString != null)
            ConfigKeyString.Value = GetDefaultValue(ConfigKeyString);

        ModdedControlsMenu.Instance.InitButtonBindingVisuals();
    }

    internal void SetWarning(bool active)
    {
        if (warning.activeSelf != active)
            warning.SetActive(active);
    }

    internal bool IsAlreadyDefault()
    {
        if (ConfigKeyCode != null)
            return ConfigKeyCode.Value == GetDefaultValue(ConfigKeyCode);

        if (ConfigKeyString != null)
            return ConfigKeyString.Value == GetDefaultValue(ConfigKeyString);

        return false;
    }

    internal void SetDefault()
    {
        if (ConfigKeyCode != null)
            ConfigKeyCode.Value = GetDefaultValue(ConfigKeyCode);

        if (ConfigKeyString != null)
            ConfigKeyString.Value = GetDefaultValue(ConfigKeyString);
    }

    //Rebind button action
    internal void RebindOperation()
    {
        if (ConfigKeyCode != null)
            ModdedControlsMenu.Instance.SelectedKeyCode = ConfigKeyCode;
        else if (ConfigKeyString != null)
            ModdedControlsMenu.Instance.SelectedKeyString = ConfigKeyString;
        else
        {
            ModConfigPlugin.Log.LogWarning($"No associated config item for {inputActionName}!!");
            return;
        }

        ModdedControlsMenu.Instance.RebindOperation();
    }

    //This refreshes the look of the setting
    //Changes color of setting value & description if not default value
    //Displays warning icon if setting is already bound to another key
    public void UpdateBindingVisuals()
    {
        bool hasOverride = !IsAlreadyDefault();

        button.Text.SetText(ConfigName);
        if (ConfigKeyCode != null)
            InputBindingDisplay.SetText(value, ConfigKeyCode.Value);

        if (ConfigKeyString != null)
            InputBindingDisplay.SetText(value, ConfigKeyString.Value);

        if (hasOverride)
        {
            value.TextMesh.color = overriddenTextColor;
            button.Text.TextMesh.color = overriddenTextColor;
        }
        else
        {
            value.TextMesh.color = defaultTextColor;
            button.Text.TextMesh.color = defaultTextColor;
        }
    }
}
