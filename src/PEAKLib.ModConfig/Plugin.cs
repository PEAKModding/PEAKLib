using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;
using PEAKLib.ModConfig.Components;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using pworld.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.UI;
using Language = LocalizedText.Language;

namespace PEAKLib.ModConfig;

/// <summary>
/// BepInEx plugin of PEAKLib.ModPlugin.
/// Depend on this with <c>[BepInDependency(ModConfigPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
[BepInDependency(UIPlugin.Id)]
public partial class ModConfigPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);
    private static List<ConfigEntryBase> EntriesProcessed { get; set; } = [];
    internal static List<ModKeyToName> ModdedKeys { get; set; } = [];
    internal static ModConfigPlugin instance = null!;

    private static List<string> _validPaths = [];
    private static List<string> GetValidKeyPaths
    {
        get
        {
            if (_validPaths.Count < 1)
                _validPaths = GenerateValidKeyPaths();

            return _validPaths;
        }
    }

    private void Awake()
    {
        instance = this;
        MonoDetourManager.InvokeHookInitializers(typeof(ModConfigPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void Start()
    {
        LoadModSettings();

        void builderDelegate(Transform parent)
        {
            Log.LogDebug("builderDelegate");
            var mainMenuHandler = parent.GetComponentInParent<MainMenuPageHandler>();
            var pauseMenuHandler = parent.GetComponentInParent<PauseMenuHandler>();

            if (mainMenuHandler == null && pauseMenuHandler == null)
                throw new Exception("Failed to get a UIPageHandler");

            var parentPage = mainMenuHandler?.GetPage<MainMenuSettingsPage>() ??pauseMenuHandler?.GetPage<PauseMenuSettingsMenuPage>();

            if (parentPage == null)
                throw new Exception("Failed to get the parent page");

            var modSettingsPage = MenuAPI.CreateChildPage("ModSettings", parentPage);

            if (mainMenuHandler != null) // we are on main menu, create a background
                modSettingsPage.CreateBackground(new Color(0, 0, 0, 0.8667f));

            modSettingsPage.SetOnOpen(() =>
            {
                //Double check if any config items have been created since initializatio
                ProcessModEntries();
            });

            var modSettingsLocalization = MenuAPI.CreateLocalization("MOD SETTINGS")
                .AddLocalization("MOD SETTINGS", Language.English)
                .AddLocalization("PARAMÈTRES DU MOD", Language.French)
                .AddLocalization("IMPOSTAZIONI MOD", Language.Italian)
                .AddLocalization("MOD-EINSTELLUNGEN", Language.German)
                .AddLocalization("AJUSTES DEL MOD", Language.SpanishSpain)
                .AddLocalization("CONFIGURACIONES DEL MOD", Language.SpanishLatam)
                .AddLocalization("CONFIGURAÇÕES DE MOD", Language.BRPortuguese)
                .AddLocalization("НАСТРОЙКИ МОДА", Language.Russian)
                .AddLocalization("НАЛАШТУВАННЯ МОДА", Language.Ukrainian)
                .AddLocalization("模组设置", Language.SimplifiedChinese)
                .AddLocalization("模組設定", Language.TraditionalChinese)
                .AddLocalization("MOD設定", Language.Japanese)
                .AddLocalization("모드 설정", Language.Korean)
                .AddLocalization("USTAWIENIA MODÓW", Language.Polish);

            var headerContainer = new GameObject("Header")
                .ParentTo(modSettingsPage)
                .AddComponent<PeakElement>()
                .SetAnchorMinMax(new Vector2(0, 1))
                .SetPosition(new Vector2(40, -40))
                .SetPivot(new Vector2(0, 1))
                .SetSize(new Vector2(360, 100));

            var newText = MenuAPI.CreateText("Mod Settings", "HeaderText")
                .SetFontSize(48)
                .ParentTo(headerContainer)
                .ExpandToParent()
                .SetLocalizationIndex(modSettingsLocalization);

            newText.Text.fontSizeMax = 48;
            newText.Text.fontSizeMin = 24;
            newText.Text.enableAutoSizing = true;
            newText.Text.alignment = TextAlignmentOptions.Center;

            var backButton = MenuAPI.CreateMenuButton("Back")
                .SetLocalizationIndex("BACK") // Peak already have a "BACK" official translation, so let's just use it
                .SetColor(new Color(0.5189f, 0.1297f, 0.1718f)) //match vanilla back
                .ParentTo(modSettingsPage)
                .SetPosition(new Vector2(120f, -160f))
                .SetWidth(120f);

            var content = new GameObject("Content")
                .AddComponent<PeakElement>()
                .ParentTo(modSettingsPage)
                .SetPivot(new Vector2(0, 1))
                .SetAnchorMin(new Vector2(0, 1))
                .SetAnchorMax(new Vector2(0, 1))
                .SetPosition(new Vector2(428, -70))
                .SetSize(new Vector2(1360, 980));

            var settingsMenu = content.gameObject.AddComponent<ModdedSettingsMenu>();
            settingsMenu.MainPage = modSettingsPage;

            if (pauseMenuHandler != null)
            {
                var controlsButton = MenuAPI.CreateMenuButton("MOD CONTROLS")
                .SetLocalizationIndex("MOD CONTROLS") //localization should exist from controls page builder
                .SetColor(new Color(0.185f, 0.394f, 0.6226f)) //same blue as main menu settings button
                .ParentTo(modSettingsPage)
                .SetPosition(new Vector2(285f, -160f))
                .SetWidth(200);

                controlsButton.OnClick(() =>
                {
                    pauseMenuHandler.TransistionToPage(ModdedControlsMenu.Instance.MainPage, new SetActivePageTransistion());
                });
            }

            MenuAPI.CreateText("Search")
                .ParentTo(modSettingsPage)
                .SetPosition(new Vector2(65f, -190f));

            var textInput = MenuAPI.CreateTextInput("SearchInput")
            .ParentTo(modSettingsPage)
            .SetSize(new Vector2(300, 70))
            .SetPosition(new Vector2(215, -275))
            .SetPlaceholder("Search here")
            .OnValueChanged(settingsMenu.SetSearch);


            modSettingsPage.SetBackButton(backButton.GetComponent<Button>()); // sadly backButton.Button doesn't work cause Awake have not being called yet

            var horizontalTabs = new GameObject("TABS")
                .ParentTo(content)
                .AddComponent<PeakHorizontalTabs>();
            horizontalTabs.RectTransform.anchoredPosition = new(110f, 0f);
            horizontalTabs.RectTransform.anchorMax = new(0.92f, 1f); //give space for labels

            var modTabsLabel = MenuAPI.CreateText("MODS")
                .ParentTo(content)
                .SetPosition(new(4, 10));

            var sectionTabsLabel = MenuAPI.CreateText("SECTIONS")
                .ParentTo(content)
                .SetPosition(new(4, -50));

            var sectionTabs = new GameObject("SectionTabs")
                .ParentTo(content)
                .AddComponent<PeakHorizontalTabs>();
            sectionTabs.RectTransform.anchoredPosition = new(175f, -55f);
            sectionTabs.RectTransform.anchorMax = new(0.87f, 1f);

            var moddedSettingsTABS = horizontalTabs.gameObject.AddComponent<ModdedSettingsTABS>();
            moddedSettingsTABS.SettingsMenu = settingsMenu;

            var modSectionTABS = sectionTabs.gameObject.AddComponent<ModdedSettingsSectionTABS>();
            modSectionTABS.SettingsMenu = settingsMenu;
            settingsMenu.SectionTabController = sectionTabs;

            var tabContent = MenuAPI.CreateScrollableContent("TabContent")
                .ParentTo(content)
                .ExpandToParent()
                .SetOffsetMax(new Vector2(0, -110f));

            settingsMenu.Content = tabContent.Content;
            settingsMenu.Tabs = moddedSettingsTABS;

            foreach (var (modName, configEntryBases) in GetModConfigEntries())
            {
                var tabButton = horizontalTabs.AddTab(modName);
                var moddedButton = tabButton.AddComponent<ModdedTABSButton>();
                moddedButton.category = modName;
                moddedButton.text = tabButton.GetComponentInChildren<TextMeshProUGUI>();
                moddedButton.SelectedGraphic = tabButton.transform.Find("Selected").gameObject;
            }

            var modSettingsButton = MenuAPI.CreatePauseMenuButton("MOD SETTINGS")
                .SetLocalizationIndex(modSettingsLocalization)
                .SetColor(new Color(0.185f, 0.394f, 0.6226f)) //same blue as main menu settings button
                .ParentTo(parent)
                .OnClick(() =>
                {
                    var handler = mainMenuHandler as UIPageHandler ?? pauseMenuHandler;

                    handler?.TransistionToPage(modSettingsPage, new SetActivePageTransistion());
                });

            modSettingsPage.gameObject.SetActive(false);

            modSettingsButton?.SetPosition(new Vector2(171, -230))
                .SetWidth(220);
        }

        void controlsBuilder(Transform parent)
        {
            Log.LogDebug("controlsBuilder");
            var pauseMenuHandler = parent.GetComponentInParent<PauseMenuHandler>();

            if (pauseMenuHandler == null)
                throw new Exception("Failed to get a UIPageHandler");

            var parentPage = parent.GetComponent<PauseMenuControlsPage>();

            if (parentPage == null)
                throw new Exception("Failed to get the parent page to create Modded Controls Page");

            var modControlsPage = MenuAPI.CreateChildPage("ModdedControlsPage", parentPage);
           
            var controlsMenu = modControlsPage.gameObject.AddComponent<ModdedControlsMenu>();

            modControlsPage.SetOnOpen(() =>
            {
                //Double check if any config items have been created since initialization
                Log.LogDebug("Modded Controls Menu Opened!");
                ProcessModEntries();
                controlsMenu.ShowControls();
            });

            controlsMenu.MainPage = modControlsPage;

            controlsMenu.RebindNotif = MenuAPI.CreateButton("RebindModdedKey")
                .ParentTo(parentPage.transform.parent)
                .ExpandToParent();

            controlsMenu.RebindNotif.Text.SetFontSize(48);
            controlsMenu.RebindNotif.gameObject.SetActive(false);

            var modControlsLocalization = MenuAPI.CreateLocalization("MOD CONTROLS")
                .AddLocalization("MOD CONTROLS", Language.English);

            var headerContainer = new GameObject("Header")
                .ParentTo(modControlsPage)
                .AddComponent<PeakElement>()
                .SetAnchorMinMax(new Vector2(0, 1))
                .SetPosition(new Vector2(40, -40))
                .SetPivot(new Vector2(0, 1))
                .SetSize(new Vector2(360, 100));

            var newText = MenuAPI.CreateText("Mod Controls", "HeaderText")
                .SetFontSize(48)
                .ParentTo(headerContainer)
                .ExpandToParent()
                .SetLocalizationIndex(modControlsLocalization);

            newText.Text.fontSizeMax = 48;
            newText.Text.fontSizeMin = 24;
            newText.Text.enableAutoSizing = true;
            newText.Text.alignment = TextAlignmentOptions.Center;

            var backButton = MenuAPI.CreateMenuButton("Back (Controls)")
                .SetLocalizationIndex("BACK") // Peak already have a "BACK" official translation, so let's just use it
                .SetColor(new Color(0.5189f, 0.1297f, 0.1718f)) //match vanilla back
                .ParentTo(modControlsPage)
                .SetPosition(new Vector2(120f, -160f))
                .SetWidth(120f);

            var modSettingsButton = MenuAPI.CreateMenuButton("MOD SETTINGS")
                .SetLocalizationIndex("MOD SETTINGS") //re-use existing localization from settings builder
                .SetColor(new Color(0.185f, 0.394f, 0.6226f)) //same blue as main menu settings button
                .ParentTo(modControlsPage)
                .SetPosition(new Vector2(285f, -160f))
                .SetWidth(220)
                .OnClick(() =>
                {
                    pauseMenuHandler.TransistionToPage(ModdedSettingsMenu.Instance.MainPage, new SetActivePageTransistion());
                });

            var restoreAllButton = MenuAPI.CreateMenuButton("Restore Defaults")
                .SetLocalizationIndex("RESTOREDEFAULTS") // Peak has an official translation for this as well
                .SetColor(new Color(0.3919f, 0.1843f, 0.6235f)) //same as default restore defaults button
                .ParentTo(modControlsPage)
                .SetPosition(new Vector2(225, -220))
                .SetWidth(300)
                .OnClick(controlsMenu.OnResetAllClicked);

            modControlsPage.SetBackButton(backButton.GetComponent<Button>()); // sadly backButton.Button doesn't work cause Awake have not being called yet

            MenuAPI.CreateText("Search")
                .ParentTo(modControlsPage)
                .SetPosition(new Vector2(65f, -245f));

            var content = new GameObject("Content")
                .AddComponent<PeakElement>()
                .ParentTo(modControlsPage)
                .SetPivot(new Vector2(0, 1))
                .SetAnchorMin(new Vector2(0, 1))
                .SetAnchorMax(new Vector2(0, 1))
                .SetPosition(new Vector2(428, -70))
                .SetSize(new Vector2(1360, 980));

            var scrollableContent = MenuAPI.CreateScrollableContent("ScrollableContent")
                .ParentTo(content)
                .ExpandToParent()
                .SetOffsetMax(new Vector2(0, -60f));

            controlsMenu.Content = scrollableContent.Content;

            var textInput = MenuAPI.CreateTextInput("SearchInput")
                .ParentTo(modControlsPage)
                .SetSize(new Vector2(300, 70))
                .SetPosition(new Vector2(215f, -330f))
                .SetPlaceholder("Search here")
                .OnValueChanged(controlsMenu.SetSearch);

            var modControlsButton = MenuAPI.CreatePauseMenuButton("MOD CONTROLS")
                .SetLocalizationIndex(modControlsLocalization)
                .SetColor(new Color(0.185f, 0.394f, 0.6226f)) //same blue as main menu settings button
                .ParentTo(parent)
                .OnClick(() =>
                {
                    pauseMenuHandler.TransistionToPage(modControlsPage, new SetActivePageTransistion());
                });

            modControlsPage.gameObject.SetActive(false);
            modControlsButton?.SetPosition(new Vector2(205, -291))
                .SetWidth(220);
        }

        //settings menu builder
        MenuAPI.AddToSettingsMenu(builderDelegate);
        //controls menu builder
        MenuAPI.AddToControlsMenu(controlsBuilder);
    }

    private static bool modSettingsLoaded = false;
    private static void LoadModSettings()
    {
        if (modSettingsLoaded) return;

        EntriesProcessed = [];
        ModdedKeys = [];
        modSettingsLoaded = true;

        ProcessModEntries();
    }

    //Processes Bepinex config items to Settings entries
    //Called during mod initialization AND whenever the mod settings page is opened
    private static void ProcessModEntries()
    {
        foreach (var (modName, configEntryBases) in GetModConfigEntries())
        {
            ModSectionNames sectionTracker = ModSectionNames.SetMod(modName);

            foreach (var configEntry in configEntryBases)
            {
                try
                {
                    //track mod entries we have processed to not duplicate setting entries
                    if (EntriesProcessed.Contains(configEntry))
                        continue;
                    else
                        EntriesProcessed.Add(configEntry);

                    sectionTracker.CheckSectionName(configEntry.Definition.Section);

                    if (configEntry.SettingType == typeof(bool))
                        SettingsHandlerUtility.AddBoolToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    else if (configEntry.SettingType == typeof(float))
                    {
                        SettingsHandlerUtility.AddFloatToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(double))
                    {
                        SettingsHandlerUtility.AddDoubleToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(int))
                    {
                        SettingsHandlerUtility.AddIntToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(string))
                    {
                        var defaultValue = configEntry.DefaultValue is string cValue ? cValue : "";

                        //checking if default value matches key path pattern
                        if (defaultValue.Length > 4)
                        {
                            if (IsValidPath(defaultValue))
                            {
                                ModKeyToName item = new(configEntry, modName);
                                ModdedKeys.Add(item);
                                Log.LogDebug($"String config with default - {defaultValue} is detected as InputAction path");
                            }
                        }

                        //dropdown box for acceptablevalue list
                        if (configEntry.Description.AcceptableValues is AcceptableValueList<string> stringList)
                        {
                            SettingsHandlerUtility.AddEnumToTab(configEntry, modName, false, newVal =>
                            {
                                configEntry.BoxedValue = newVal;
                            });
                            return;
                        }

                        SettingsHandlerUtility.AddStringToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(KeyCode))
                    {
                        if (configEntry is ConfigEntry<KeyCode> entry)
                        {
                            ModKeyToName item = new(entry, modName);
                            ModdedKeys.Add(item);
                        }

                        SettingsHandlerUtility.AddKeybindToTab(configEntry, modName, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType.IsEnum)
                    {
                        SettingsHandlerUtility.AddEnumToTab(configEntry, modName, true, newVal =>
                        {
                            if (Enum.TryParse(configEntry.SettingType, newVal, out var value))
                                configEntry.BoxedValue = value;
                        });
                    }
                    else // Warn about missing SettingTypes
                        Log.LogWarning($"Missing SettingType: [Mod: {modName}] {configEntry.Definition.Key} (Type: {configEntry.SettingType})");
                }

                catch (Exception e)
                {
                    Log.LogError(e);
                }
            }
        }
        
    }

    private static bool IsValidPath(string path)
    {
        path = path.Replace("<", "").Replace(">", "");
        return GetValidKeyPaths.Any(x => x.Contains(path, StringComparison.InvariantCultureIgnoreCase));
    }

    //get valid potential keypaths
    private static List<string> GenerateValidKeyPaths()
    {
        List<string> result = [];
        List<string> doNotAdd = ["/Keyboard/anyKey"];
        foreach (var device in InputSystem.devices)
        {
            if (device == null)
                continue;

            foreach (var control in device.allControls)
            {
                if (doNotAdd.Any(d => d.Equals(control.path, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                result.Add(control.path);
            }
        }

        return result;
    }

    // From https://github.com/IsThatTheRealNick/REPOConfig/blob/main/REPOConfig/ConfigMenu.cs#L453
    private static Dictionary<string, ConfigEntryBase[]> GetModConfigEntries()
    {
        var configs = new Dictionary<string, ConfigEntryBase[]>();

        foreach (var plugin in Chainloader.PluginInfos.Values.OrderBy(p => p.Metadata.Name))
        {
            var configEntries = new List<ConfigEntryBase>();

            foreach (var configEntryBase in plugin.Instance.Config.Select(configEntry => configEntry.Value))
            {
                var tags = configEntryBase.Description?.Tags;

                if (tags != null && tags.Contains("Hidden")) continue;

                configEntries.Add(configEntryBase);
            }

            if (configEntries.Count > 0)
                configs.TryAdd(FixNaming(plugin.Metadata.Name), [.. configEntries]);
        }

        return configs;
    }

    private static string FixNaming(string input)
    {
        input = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        input = Regex.Replace(input, "([A-Z])([A-Z][a-z])", "$1 $2");
        input = Regex.Replace(input, @"\s+", " ");
        input = Regex.Replace(input, @"([A-Z]\.)\s([A-Z]\.)", "$1$2");

        return input.Trim();
    }
}
