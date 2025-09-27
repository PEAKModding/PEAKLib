using BepInEx.Configuration;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.ControllerSupport;
using Zorro.UI;

namespace PEAKLib.ModConfig.Components;

internal class ModdedControlsMenu : PeakElement
{

    //original unused stuff
    public GameObject[] keyboardOnlyObjects = [];
    public GameObject[] controllerOnlyObjects = [];
    // ---

    public static ModdedControlsMenu Instance = null!;
    public List<ModdedRebindKeyCode> controlsMenuButtons = [];
    public List<GameObject> ModLabels = [];

    private string search = "";

    internal UIPageHandler pageHandler = null!;

    internal bool RebindInProgress = false;
    internal PeakChildPage MainPage = null!;
    internal PeakButton RebindNotif = null!;
    internal ConfigEntry<KeyCode> Selected = null!;
    internal Transform Content { get; set; } = null!;

    public void Awake()
    {
        Instance = this;
        if (pageHandler == null)
        {
            pageHandler = GetComponentInParent<UIPageHandler>();
        }
        controlsMenuButtons = [];
        InitButtons();
    }

    internal void OnResetClicked()
    {
        ModConfigPlugin.Log.LogMessage("Resetting all modded settings!");
        controlsMenuButtons.ForEach(b => 
        {
            if (b.ConfigEntry.Value == (KeyCode)b.ConfigEntry.DefaultValue)
                return;

            b.ConfigEntry.Value = (KeyCode)b.ConfigEntry.DefaultValue;
            // triggering the animator to make it look like all the reset buttons were pressed
            b.reset.GetComponent<Button>().animator.SetBool("Pressed", true);
            b.reset.GetComponent<Button>().animator.SetBool("Disabled", true); 
        });
        ShowControls();
    }

    public void OnDeviceChange(InputScheme scheme)
    {
        switch (scheme)
        {
            case InputScheme.KeyboardMouse:
                {
                    GameObject[] array = keyboardOnlyObjects;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetActive(value: true);
                    }

                    array = controllerOnlyObjects;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetActive(value: false);
                    }

                    break;
                }
            case InputScheme.Gamepad:
                {
                    GameObject[] array = keyboardOnlyObjects;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetActive(value: false);
                    }

                    array = controllerOnlyObjects;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetActive(value: true);
                    }

                    break;
                }
        }

        InitButtonBindingVisuals(scheme);
        LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
    }

    public void InitButtons()
    {
        if (pageHandler == null)
        {
            pageHandler = GetComponentInParent<UIPageHandler>();
        }

        var buttonsToCreate = ModConfigPlugin.ModdedKeybinds;
        if (controlsMenuButtons.Count > 0)
        {
            foreach(var button in controlsMenuButtons)
                buttonsToCreate.Remove(button.ConfigEntry);
        }

        var ordered = buttonsToCreate.OrderBy(x => x.Value);

        foreach (var config in ordered)
            AddControlMenuButton(config.Key, config.Value);
    }

    private void AddControlMenuButton(ConfigEntry<KeyCode> configEntry, string ModName) 
    {
        if (Content == null)
            return;

        if(ModLabels.FirstOrDefault(x => x.name == ModName) is not GameObject modName)
        {
            var modText = MenuAPI.CreateText(ModName, ModName)
                .SetFontSize(36f)
                .SetColor(Color.green)
                .ParentTo(Content);
            modText.TextMesh.alignment = TMPro.TextAlignmentOptions.Center;
            

            modName = modText.gameObject;
            ModLabels.Add(modName);
        }

        GameObject control = new($"({ModName}) {configEntry.Definition.Key}");
        var item = control.AddComponent<ModdedRebindKeyCode>();
        item.Label = modName;
        item.Setup(configEntry, ModName);
        control.transform.localPosition = Vector3.zero;
        control.transform.SetParent(Content);
        controlsMenuButtons.Add(item);
    }

    public void InitButtonBindingVisuals(InputScheme scheme)
    {
        //disable labels before refresh
        foreach (var item in ModLabels)
            item.SetActive(false);

        for (int i = 0; i < controlsMenuButtons.Count; i++)
        {
            var isSearching = !string.IsNullOrEmpty(search);
            if (isSearching)
            {
                controlsMenuButtons[i].gameObject.SetActive(controlsMenuButtons[i].inputActionName.Contains(search, StringComparison.InvariantCultureIgnoreCase));
            }
            else if(!controlsMenuButtons[i].gameObject.activeSelf)
                controlsMenuButtons[i].gameObject.SetActive(true);

            controlsMenuButtons[i].UpdateBindingVisuals(controlsMenuButtons, scheme);

            //enable labels for active control items
            if (controlsMenuButtons[i].gameObject.activeSelf)
                controlsMenuButtons[i].Label.SetActive(true);
        }

        //Below prob not needed
        /*
        for (int i = 0; i < ModLabels.Count; i++)
        {
            if (ModLabels[i].transform.childCount == 0)
            {
                ModLabels[i].gameObject.SetActive(false);
                continue;
            }
                

            bool showLabel = false;
            for (int x = 0; x < ModLabels[i].transform.childCount; i++)
            {
                if (ModLabels[i].transform.GetChild(x).gameObject.activeSelf)
                {
                    showLabel = true;
                    break;
                }    
            }

            ModLabels[i].gameObject.SetActive(showLabel);
        }*/
    }

    public void SetSearch(string search)
    {
        this.search = search.ToLower();
        ShowControls(); // just to update the settings
    }

    public void ShowControls()
    {
        RebindNotif?.gameObject.SetActive(false);
        InitButtons();
        OnDeviceChange(InputHandler.GetCurrentUsedInputScheme());
    }

    public void RebindOperation()
    {
        if (Selected == null)
            return;

        transform.gameObject.SetActive(false);
        RebindInProgress = true;
        RebindNotif.Text.TextMesh.text = LocalizedText.GetText("PROMPT_REBIND").Replace("@", Selected.Definition.Key.ToString());
        RebindNotif.gameObject.SetActive(true);

        GUIManager.instance.StartCoroutine(AwaitControl());
    }

    public IEnumerator AwaitControl()
    {
        var action_pause = InputSystem.actions.FindAction("Pause");
        while (RebindInProgress)
        {
            if(action_pause.WasPressedThisFrame())
            {
                ModConfigPlugin.Log.LogDebug("Exiting new bind operation!");
                RebindNotif.gameObject.SetActive(false);
                RebindInProgress = false;
                pageHandler.TransistionToPage(MainPage, new SetActivePageTransistion());
                ShowControls();
                yield break;
            }

            if (Input.anyKeyDown)
            {
                ModConfigPlugin.Log.LogDebug("Setting new bind value!");
                KeyCode[] keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
                KeyCode key = keys.FirstOrDefault(x => Input.GetKeyDown(x));
                Selected.Value = key;
                RebindInProgress = false;
                RebindNotif.gameObject.SetActive(false);
                transform.gameObject.SetActive(true);
                ShowControls();
                yield break;
            }

            yield return null;
        }
    }
}
