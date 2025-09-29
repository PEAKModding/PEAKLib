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
    public List<ModdedRebindKey> controlsMenuButtons = [];
    public List<GameObject> ModLabels = [];

    private string search = "";

    internal UIPageHandler pageHandler = null!;

    internal bool RebindInProgress = false;
    internal PeakChildPage MainPage = null!;
    internal PeakButton RebindNotif = null!;
    internal ConfigEntry<KeyCode> SelectedKeyCode = null!;
    internal ConfigEntry<string> SelectedKeyString = null!;
    internal Transform Content { get; set; } = null!;

    private static readonly InputAction Dummy = new("ModConfig Dummy InputAction for Rebinding");

    public void Awake()
    {
        ModConfigPlugin.Log.LogDebug("ModdedControlsMenu Awake");
        Instance = this;
        if (pageHandler == null)
        {
            pageHandler = GetComponentInParent<UIPageHandler>();
        }

        InitButtons();
        if(Dummy.bindings.Count < 1)
            Dummy.AddBinding("/Keyboard/anyKey");
    }

    internal void OnResetAllClicked()
    {
        ModConfigPlugin.Log.LogMessage("Resetting all modded settings!");
        controlsMenuButtons.ForEach(b => 
        {
            if (b.IsAlreadyDefault())
                return;

            b.SetDefault();
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

        InitButtonBindingVisuals();
        LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
    }

    public void InitButtons()
    {
        if (pageHandler == null)
        {
            pageHandler = GetComponentInParent<UIPageHandler>();
        }

        //keycodes & keystrings
        List<ModKeyToName> keysToAdd = [.. ModConfigPlugin.ModdedKeys];
        if (controlsMenuButtons.Count > 0)
        {
            foreach(var button in controlsMenuButtons)
            {
                if (button.ConfigKeyCode != null)
                    keysToAdd = ModKeyToName.RemoveKey(keysToAdd, button.ConfigKeyCode);
                if (button.ConfigKeyString != null)
                    keysToAdd = ModKeyToName.RemoveKey(keysToAdd, button.ConfigKeyString);
            }
        }

        foreach (var config in keysToAdd)
            AddControlMenuButton(config.KeyBind, config.ModName);
    }

    private void AddControlMenuButton(ConfigEntryBase configEntry, string ModName) 
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
        var item = control.AddComponent<ModdedRebindKey>();
        item.Label = modName;
        item.Setup(configEntry, ModName);
        control.transform.localPosition = Vector3.zero;
        control.transform.SetParent(Content);
        controlsMenuButtons.Add(item);
    }

    public void InitButtonBindingVisuals()
    {
        //disable labels before refresh
        foreach (var item in ModLabels)
            item.SetActive(false);

        controlsMenuButtons = [.. controlsMenuButtons.OrderBy(x => x.Label.name)];

        for (int i = 0; i < controlsMenuButtons.Count; i++)
        {
            var isSearching = !string.IsNullOrEmpty(search);
            if (isSearching)
            {
                controlsMenuButtons[i].gameObject.SetActive(controlsMenuButtons[i].inputActionName.Contains(search, StringComparison.InvariantCultureIgnoreCase) || controlsMenuButtons[i].Label.name.Contains(search, StringComparison.InvariantCultureIgnoreCase));
            }
            else if(!controlsMenuButtons[i].gameObject.activeSelf)
                controlsMenuButtons[i].gameObject.SetActive(true);

            controlsMenuButtons[i].UpdateBindingVisuals();

            //enable labels for active control items
            if (controlsMenuButtons[i].gameObject.activeSelf)
                controlsMenuButtons[i].Label.SetActive(true);
        }

        //warning game object behavior
        //done after values have been updated to avoid issues
        DuplicatesCheck();
    }

    public void DuplicatesCheck()
    {
        foreach (ModdedRebindKey btn in controlsMenuButtons)
        {
            if (btn.value.Text.text.Equals("None", StringComparison.InvariantCultureIgnoreCase))
            {
                btn.SetWarning(false);
                continue;
            }

            if (controlsMenuButtons.Any(x => x.value.Text.text.Equals(btn.value.Text.text, StringComparison.InvariantCultureIgnoreCase) && x != btn))
                btn.SetWarning(true);
            else
                btn.SetWarning(false);
        } 
    }

    public void SetSearch(string search)
    {
        this.search = search.ToLower();
        ShowControls(); // just to update the settings
    }

    public void ShowControls()
    {
        SelectedKeyCode = null!;
        SelectedKeyString = null!;
        RebindNotif?.gameObject.SetActive(false);
        InitButtons();
        OnDeviceChange(InputHandler.GetCurrentUsedInputScheme());
    }

    public void RebindOperation()
    {
        if (SelectedKeyCode == null && SelectedKeyString == null)
            return;

        transform.gameObject.SetActive(false);
        RebindInProgress = true;
        
        RebindNotif.gameObject.SetActive(true);

        if(SelectedKeyCode != null)
        {
            RebindNotif.Text.TextMesh.text = LocalizedText.GetText("PROMPT_REBIND").Replace("@", SelectedKeyCode.Definition.Key.ToString());
            GUIManager.instance.StartCoroutine(AwaitKeyCode());
        }
            
        if(SelectedKeyString != null)
        {
            RebindNotif.Text.TextMesh.text = LocalizedText.GetText("PROMPT_REBIND").Replace("@", SelectedKeyString.Definition.Key.ToString());
            GUIManager.instance.StartCoroutine(AwaitKeyPath());
        }
    }

    public IEnumerator AwaitKeyCode()
    {
        //needed to stop modded controls page from closing on cancel rebind
        InputActionRebindingExtensions.RebindingOperation rebindOperation = Dummy.PerformInteractiveRebinding(0);
        rebindOperation.Start();
        while (RebindInProgress)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                rebindOperation.Cancel();
                rebindOperation.Dispose();
                RebindCancel();
                yield break;
            }

            if (Input.anyKeyDown)
            {
                rebindOperation.Cancel();
                rebindOperation.Dispose();
                ModConfigPlugin.Log.LogDebug("Setting new bind value!");
                KeyCode[] keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
                KeyCode key = keys.FirstOrDefault(x => Input.GetKeyDown(x));
                SelectedKeyCode.Value = key;
                RebindInProgress = false;
                RebindNotif.gameObject.SetActive(false);
                transform.gameObject.SetActive(true);
                ShowControls();
                yield break;
            }

            yield return null;
        }
    }

    private void RebindCancel() 
    {
        ModConfigPlugin.Log.LogDebug("Exiting new bind operation!");
        RebindNotif.gameObject.SetActive(false);
        RebindInProgress = false;
        transform.gameObject.SetActive(true);
        ShowControls();
    }

    public IEnumerator AwaitKeyPath()
    {
        var action_pause = InputSystem.actions.FindAction("Pause");
        InputActionRebindingExtensions.RebindingOperation rebindOperation = Dummy.PerformInteractiveRebinding(0)
            .WithoutGeneralizingPathOfSelectedControl()
            .OnComplete(operation =>  
            {
                ModConfigPlugin.Log.LogDebug("Setting new bind value!");
                SelectedKeyString.Value = Dummy.bindings[0].overridePath;
                RebindInProgress = false;
                RebindNotif.gameObject.SetActive(false);
                transform.gameObject.SetActive(true);
                ShowControls();
                operation.Dispose();
            })
            .OnCancel(operation =>
            {
                RebindCancel();
                operation.Dispose();
            });

        rebindOperation.Start();

        while (RebindInProgress)
        {
            if (action_pause.WasPressedThisFrame() && rebindOperation != null && rebindOperation.started && !rebindOperation.completed)
            {
                rebindOperation.Dispose();
                RebindCancel();
                yield break;
            }

            yield return null;
        }
    }
}
