using PEAKLib.UI.Elements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    internal PeakChildPage MainPage = null!;

    private void Awake()
    {
        Instance = this;
    }

    public void SetSearch(string search)
    {
        this.search = search.ToLower();
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