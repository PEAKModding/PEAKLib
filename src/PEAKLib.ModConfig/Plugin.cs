using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using PEAKLib.ModConfig.Components;
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
    private static List<ConfigEntryBase> EntriesProcessed = [];

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(ModConfigPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void Start()
    {
        LoadModSettings();

        void builderDelegate(Transform parent)
        {
            var isTitleScreen = SceneManager.GetActiveScene().name == "Title";
            MenuWindow settingMenu = isTitleScreen
                ? parent.GetComponentInParent<PauseMainMenu>()
                : parent.GetComponentInParent<PauseSettingsMenu>();

            var modSettingsPage = MenuAPI.CreatePage("ModSettings")
                .CreateBackground(isTitleScreen ? new Color(0, 0, 0, 0.8667f) : null)
                .SetOnClose(settingMenu.Open);

            modSettingsPage.SetOnOpen(() =>
            {
                //Double check if any config items have been created since initialization
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
                .AddLocalization("모드 설정", Language.Korean);

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

            MenuAPI.CreateMenuButton("Back")
                .SetLocalizationIndex("BACK") // Peak already have a "BACK" official translation, so let's just use it
                .SetColor(new Color(1, 0.5f, 0.2f))
                .ParentTo(modSettingsPage)
                .SetPosition(new Vector2(225, -180))
                .SetWidth(200)
                .OnClick(modSettingsPage.Close);

            MenuAPI.CreateText("Search")
                .ParentTo(modSettingsPage)
                .SetPosition(new Vector2(90, -210));

            var content = new GameObject("Content")
                .AddComponent<PeakElement>()
                .ParentTo(modSettingsPage)
                .SetPivot(new Vector2(0, 1))
                .SetAnchorMin(new Vector2(0, 1))
                .SetAnchorMax(new Vector2(0, 1))
                .SetPosition(new Vector2(428, -70))
                .SetSize(new Vector2(1360, 980));

            var settingsMenu = content.gameObject.AddComponent<ModdedSettingsMenu>();


            var textInput = MenuAPI.CreateTextInput("SearchInput")
                .ParentTo(modSettingsPage)
                .SetSize(new Vector2(300, 70))
                .SetPosition(new Vector2(230, -300))
                .SetPlaceholder("Search here")
                .OnValueChanged(settingsMenu.SetSearch);

            var horizontalTabs = new GameObject("TABS")
                .ParentTo(content)
                .AddComponent<PeakHorizontalTabs>();

            var moddedSettingsTABS = horizontalTabs.gameObject.AddComponent<ModdedSettingsTABS>();
            moddedSettingsTABS.SettingsMenu = settingsMenu;

            var tabContent = MenuAPI.CreateScrollableContent("TabContent")
                .ParentTo(content)
                .ExpandToParent()
                .SetOffsetMax(new Vector2(0, -60f));

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
                .SetColor(new Color(0.15f, 0.75f, 0.85f))
                .ParentTo(parent)
                .OnClick(() =>
                {
                    UIInputHandler.SetSelectedObject(null);
                    settingMenu?.Close();

                    if (!isTitleScreen && settingMenu is PauseSettingsMenu pauseSettingsMenu)
                        pauseSettingsMenu.optionsMenu?.Close();
                    
                    modSettingsPage.Open();
                });

            if (modSettingsButton != null)
                modSettingsButton
                .SetPosition(new Vector2(171, -230))
                .SetWidth(220);
        }

        MenuAPI.AddToSettingsMenu(builderDelegate);
    }

    private static bool modSettingsLoaded = false;
    private static void LoadModSettings()
    {
        if (modSettingsLoaded) return;

        EntriesProcessed = [];
        modSettingsLoaded = true;

        ProcessModEntries();
    }

    //Processes Bepinex config items to Settings entries
    //Called during mod initialization AND whenever the mod settings page is opened
    private static void ProcessModEntries()
    {
        foreach (var (modName, configEntryBases) in GetModConfigEntries())
        {
            foreach (var configEntry in configEntryBases)
            {
                try
                {
                    //track mod entries we have processed to not duplicate setting entries
                    if (EntriesProcessed.Contains(configEntry))
                        continue;
                    else
                        EntriesProcessed.Add(configEntry);

                    if (configEntry.SettingType == typeof(bool))
                    {
                        var defaultValue = configEntry.DefaultValue is bool dValue && dValue;
                        var currentValue = configEntry.BoxedValue is bool bValue && bValue;

                        SettingsHandlerUtility.AddBoolToTab(configEntry.Definition.Key, defaultValue, modName, currentValue, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(float))
                    {
                        var defaultValue = configEntry.DefaultValue is float cValue ? cValue : 0f;
                        var currentValue = configEntry.BoxedValue is float bValue ? bValue : 0f;

                        float minValue = 0f;
                        float maxValue = 1000f;

                        if (configEntry.Description.AcceptableValues is AcceptableValueRange<float> range)
                        {
                            minValue = range.MinValue;
                            maxValue = range.MaxValue;
                        }

                        SettingsHandlerUtility.AddFloatToTab(configEntry.Definition.Key, defaultValue, modName, minValue, maxValue, currentValue, newVal => configEntry.BoxedValue = newVal);
                    }

                    else if (configEntry.SettingType == typeof(int))
                    {
                        var defaultValue = configEntry.DefaultValue is int cValue ? cValue : 0;
                        var currentValue = configEntry.BoxedValue is int bValue ? bValue : 0;
                        SettingsHandlerUtility.AddIntToTab(configEntry.Definition.Key, defaultValue, modName, currentValue, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(string))
                    {
                        var defaultValue = configEntry.DefaultValue is string cValue ? cValue : "";
                        var currentValue = configEntry.BoxedValue is string bValue ? bValue : "";
                        SettingsHandlerUtility.AddStringToTab(configEntry.Definition.Key, defaultValue, modName, currentValue, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType == typeof(KeyCode))
                    {
                        var defaultValue = configEntry.DefaultValue is KeyCode cValue ? cValue : KeyCode.None;
                        var currentValue = configEntry.BoxedValue is KeyCode bValue ? bValue : KeyCode.None;

                        SettingsHandlerUtility.AddKeybindToTab(configEntry.Definition.Key, defaultValue, modName, currentValue, newVal => configEntry.BoxedValue = newVal);
                    }
                    else if (configEntry.SettingType.IsEnum)
                    {
                        var defaultValue = configEntry.DefaultValue is object cValue ? cValue : Enum.ToObject(configEntry.SettingType, 0);
                        var currentValue = configEntry.BoxedValue is object bValue ? bValue : Enum.ToObject(configEntry.SettingType, 0);

                        var defaultValueName = Enum.GetName(configEntry.SettingType, defaultValue);
                        var currentValueName = Enum.GetName(configEntry.SettingType, currentValue);
                        var options = new List<string>(Enum.GetNames(configEntry.SettingType));

                        SettingsHandlerUtility.AddEnumToTab(configEntry.Definition.Key, options, modName, currentValueName, newVal =>
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
