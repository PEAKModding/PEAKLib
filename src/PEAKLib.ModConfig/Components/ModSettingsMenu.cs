using PEAKLib.UI.Elements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zorro.Settings;

namespace PEAKLib.ModConfig.Components;

internal class ModdedSettingsMenu : MonoBehaviour
{
    private void OnEnable()
    {
        RefreshSettings();

        if (Tabs != null && Tabs.selectedButton != null)
            Tabs.Select(Tabs.selectedButton);

    }

    internal static ModdedSettingsMenu Instance { get; private set; } = null!;

    public ModdedSettingsTABS Tabs { get; set; } = null!;

    public Transform Content { get; set; } = null!;

    private List<IExposedSetting>? settings;

    private readonly List<SettingsUICell> m_spawnedCells = [];

    private Coroutine? m_fadeInCoroutine;

    private string search = "";
    private string selectedSection = "";
    public PeakHorizontalTabs SectionTabController = null!;

    internal PeakChildPage MainPage = null!;

    private void Awake()
    {
        ModConfigPlugin.Log.LogDebug("ModdedSettingsMenu Awake");
        Instance = this;
    }

    public void SetSearch(string query)
    {
        search = query.ToLower();
        ShowSettings(Tabs.selectedButton.category); // just to update the settings
    }

    public void SetSection(string section)
    {
        selectedSection = section;
        ShowSettings(Tabs.selectedButton.category); // just to update the settings
    }

    public void ShowSettings(string category)
    {
        if (m_fadeInCoroutine != null)
        {
            StopCoroutine(m_fadeInCoroutine);
            m_fadeInCoroutine = null;
        }

        foreach (SettingsUICell spawnedCell in m_spawnedCells)
            Destroy(spawnedCell.gameObject);

        m_spawnedCells.Clear();
        RefreshSettings();
            
        if (settings == null) return;

        var isSearching = !string.IsNullOrEmpty(search);
        foreach (IExposedSetting item in from setting in settings
                                         where setting != null && setting is IBepInExProperty
                                         let searchCheck = isSearching && setting.GetDisplayName()?.ToLower()?.Contains(search) == true
                                         let notSearching = !isSearching && setting.GetCategory() == category.ToString()
                                         where searchCheck || notSearching
                                         where setting is not IConditionalSetting conditionalSetting || conditionalSetting.ShouldShow()
                                         select setting)

        {
            if (Templates.SettingsCellPrefab == null)
            {
                ModConfigPlugin.Log.LogError("SettingsCellPrefab has not been loaded.");
                return;
            }

            if (item is not Setting setting)
            {
                ModConfigPlugin.Log.LogError("Invalid IExposedSetting");
                return;
            }

            if (!string.IsNullOrEmpty(selectedSection)) //skip if selected section is empty/null
            {
                if (item is IBepInExProperty bep)
                {
                    //update assigned value from configbase
                    bep.RefreshValueFromConfig();

                    if (!bep.ConfigBase.Definition.Section.Equals(selectedSection, System.StringComparison.InvariantCultureIgnoreCase))
                        continue; //skip setting that is not in current selected section
                }
                else
                    continue; //skip setting that is not bepinex base
            }

            SettingsUICell component = Instantiate(Templates.SettingsCellPrefab, Content).GetComponent<SettingsUICell>();
            m_spawnedCells.Add(component);
            // component.Setup(item as Setting);

            // temporary fix - uncomment component.Setup and remove the region when they set printDebug default to false in LocalizedText.GetText(string id, bool printDebug = true)

            #region temporary fix
            component.m_text.text = item.GetDisplayName();
            component.m_canvasGroup = component.GetComponent<CanvasGroup>();
            component.m_canvasGroup.alpha = 0f;

            Instantiate(setting.GetSettingUICell(), component.m_settingsContentParent).GetComponent<SettingInputUICell>().Setup(setting, GameHandler.Instance.SettingsHandler);
            #endregion
        }

        m_fadeInCoroutine = StartCoroutine(FadeInCells());
    }

    public void RefreshSettings()
    {
        if (GameHandler.Instance != null)
            settings = GameHandler.Instance.SettingsHandler.GetSettingsThatImplements<IExposedSetting>();

    }

    public void UpdateSectionTabs(string modName)
    {
        if(ModSectionNames.TryGetModSections(modName, out List<string> sections))
        {
            if(SectionTabController.Tabs.Count > 0)
            {
                //Remove existing tabs
                for(int i = SectionTabController.Tabs.Count - 1; i >= 0; i--)
                    SectionTabController.DeleteTab(SectionTabController.Tabs[i].name);
            }

            List<ModdedTABSButton> sectionButtons = [];
            foreach (string section in sections)
            {
                GameObject tab = SectionTabController.AddTab(section);
                var sectionButton = tab.AddComponent<ModdedTABSButton>();
                sectionButton.category = section;
                sectionButton.text = tab.GetComponentInChildren<TextMeshProUGUI>();
                sectionButton.SelectedGraphic = tab.transform.Find("Selected").gameObject;
                sectionButtons.Add(sectionButton);      
            }

            sectionButtons[0].ButtonClicked();
        }
    }

    private IEnumerator FadeInCells()
    {
        int i = 0;
        foreach (SettingsUICell spawnedCell in m_spawnedCells)
        {
            spawnedCell.FadeIn();
            yield return new WaitForSecondsRealtime(0.05f);
            i++;
        }

        m_fadeInCoroutine = null;
    }
}