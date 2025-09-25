using BepInEx.Configuration;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
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

    internal void Setup(ConfigEntry<KeyCode> entry, string ModName)
    {
        ConfigEntry = entry;
        inputActionName = $"({ModName}) {entry.Definition.Key}";

        button = MenuAPI.CreateButton(inputActionName)
            .ParentTo(transform)
            .SetSize(new(950f, 60f))
            .OnClick(RebindOperation);

        button.Button.targetGraphic.color = new(0.3396f, 0.204f, 0.1362f, 0.7294f); //match original controls page
        button.Text.TextMesh.alignment = TextAlignmentOptions.TopLeft;
        button.Text.TextMesh.margin = new(15f, 5f, 0, 0);
        button.Text.TextMesh.color = defaultTextColor;
        button.Text.TextMesh.enableAutoSizing = true;
        button.Text.TextMesh.fontSizeMin = 18f;
        button.Text.TextMesh.fontSizeMax = 30f;

        value = MenuAPI.CreateText(ConfigEntry.Value.ToString())
            .ParentTo(transform)
            .SetPosition(new(925f, -24f))
            .SetFontSize(30f)
            .SetColor(defaultTextColor);

        warning = GameObject.Instantiate(Templates.BindWarningButton!, transform);
        warning.transform.localPosition = new(-500f, 0f, 0f);
        warning.SetActive(false);

        reset = GameObject.Instantiate(Templates.ResetBindButton!, transform);
        reset.GetComponent<Button>().onClick.RemoveAllListeners();
        reset.GetComponent<Button>().onClick.AddListener(ResetThis);
        reset.transform.localPosition = new(445f, 0f, 0f);
    }

    private void ResetThis()
    {
        ConfigEntry.Value = (KeyCode)ConfigEntry.DefaultValue;
        ModdedControlsMenu.Instance.InitButtonBindingVisuals(InputHandler.GetCurrentUsedInputScheme());
    }

    internal void RebindOperation()
    {
        ModdedControlsMenu.Instance.Selected = ConfigEntry;
        ModdedControlsMenu.Instance.RebindOperation();
    }

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

        //string display = ConfigEntry.Definition.Key + $": <color=green>{ConfigEntry.Value}</color>";

        warning.SetActive(warn);
        button.Text.SetText(ConfigEntry.Definition.Key);
        value.SetText($"{ConfigEntry.Value}");

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
