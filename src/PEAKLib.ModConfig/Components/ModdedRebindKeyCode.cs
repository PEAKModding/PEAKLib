using BepInEx.Configuration;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.ControllerSupport;

namespace PEAKLib.ModConfig.Components;
internal class ModdedRebindKeyCode : PeakElement
{
    public string inputActionName = string.Empty;
    internal GameObject warning = null!;
    internal GameObject rebind = null!;
    internal GameObject reset = null!;

    internal Color defaultTextColor = new(0.8745f, 0.8549f, 0.7608f);
    internal Color overriddenTextColor = new(0.8679f, 0.7459f, 0.3316f);

    internal ConfigEntry<KeyCode> ConfigEntry = null!;
    private PeakButton button = null!;
    private PeakText value = null!;
    internal GameObject Label = null!;

    //this shit was a LOT of trial and error to get things looking better in a uniform way
    internal void Setup(ConfigEntry<KeyCode> entry, string ModName)
    {
        ConfigEntry = entry;
        inputActionName = $"({ModName}) {entry.Definition.Key}";

        button = MenuAPI.CreateButton(inputActionName)
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
        rightParent.GetComponent<RectTransform>().anchoredPosition = new(450f, 0f);
        
        value = MenuAPI.CreateText(ConfigEntry.Value.ToString())
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
        layoutelement.preferredWidth = 66f;
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

    //Reset button action, resets the setting to the default value
    private void ResetThis()
    {
        ConfigEntry.Value = (KeyCode)ConfigEntry.DefaultValue;
        ModdedControlsMenu.Instance.InitButtonBindingVisuals(InputHandler.GetCurrentUsedInputScheme());
    }

    //Rebind button action
    internal void RebindOperation()
    {
        ModdedControlsMenu.Instance.Selected = ConfigEntry;
        ModdedControlsMenu.Instance.RebindOperation();
    }

    //This refreshes the look of the setting
    //Changes color of setting value & description if not default value
    //Displays warning icon if setting is already bound to another key
    public void UpdateBindingVisuals(List<ModdedRebindKeyCode> allButtons, InputScheme scheme)
    {
        bool hasOverride = ConfigEntry.Value != (KeyCode)ConfigEntry.DefaultValue;
        bool warn = false;
        allButtons.RemoveAll(a => a == null!);
        foreach (ModdedRebindKeyCode pauseMenuRebindButton in allButtons)
        {
            if (pauseMenuRebindButton == this)
                continue;

            if (pauseMenuRebindButton.ConfigEntry.Value == ConfigEntry.Value && pauseMenuRebindButton.ConfigEntry.Value != KeyCode.None)
            {
                warn = true;
            }
        }

        warning.SetActive(warn);
        button.Text.SetText(ConfigEntry.Definition.Key);
        string key = GetValidKeyValue(ConfigEntry.Value);
        value.TextMesh.spriteAsset = Templates.KeyboardSpriteSheet; //no controller support currently
        value.SetText($"{key}");

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

    //Translation for KeyCode to valid sprite tag
    private static string GetValidKeyValue(KeyCode key)
    {
        string search = key.ToString();
        List<KeyCode> Numbers = [KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9];

        List<KeyCode> MouseKeys = [KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6];

        if (MouseKeys.Contains(key))
        {
            if (key == KeyCode.Mouse0)
                return "<sprite=109 tint=1>";

            if (key == KeyCode.Mouse1)
                return "<sprite=110 tint=1>";

            if (key == KeyCode.Mouse2)
                return "<sprite=111 tint=1>";

            return key.ToString();
        }

        //does not have a sprite (per darmuh's keyboard)
        //KeyCode.None, KeyCode.Print, KeyCode.ScrollLock, KeyCode.Pause, KeyCode.Numlock, KeyCode.LeftApple (windows key), KeyCode.RightMeta (cmd key)

        if (Numbers.Contains(key))
            search = search.ToString().Replace("Alpha", "");

        if (search.ToString().Contains("keypad", StringComparison.InvariantCultureIgnoreCase))
            search = search.ToString().Replace("Keypad", "numpad");

        if (search.ToString().Contains("control", StringComparison.InvariantCultureIgnoreCase))
            search = search.ToString().Replace("Control", "Ctrl");

        if (search.ToString().Contains("return", StringComparison.InvariantCultureIgnoreCase))
            search = search.ToString().Replace("Return", "Enter");

        if (search.ToString().Contains("backquote", StringComparison.InvariantCultureIgnoreCase))
            search = search.ToLowerInvariant();

        //this fixes most conversions that are not covered above
        search = Char.ToLower(search.ToString()[0]) + search.ToString()[1..];

        //get the actual sprite tag or return original key string if no matching sprite tag
        if (InputSpriteData.Instance.inputPathToSpriteTagKeyboard.TryGetValue(search, out string sprite))
            return sprite;
        else
            return key.ToString();

        //base game provides the following sprite tag if an unknown input is provided
        //"<sprite=124 tint=1>"
        //it's basically just a key with ?? on it
        //We could provide this instead of the string, but I think the string works better in our case

    }
}
