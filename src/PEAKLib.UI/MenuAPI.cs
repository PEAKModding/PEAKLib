using PEAKLib.UI.Elements;
using UnityEngine;

namespace PEAKLib.UI;

/// <summary>
/// Main API to create everything related to UI
/// </summary>
public static class MenuAPI
{
    internal static BuilderDelegate? pauseMenuBuilderDelegate, mainMenuBuilderDelegate;

    /// <summary>
    /// Delegate to create elements
    /// </summary>
    /// <param name="transform">The target transform</param>
    public delegate void BuilderDelegate(Transform transform);

    /// <summary>
    /// Add element(s) to Main Menu
    /// </summary>
    /// <param name="builderDelegate"></param>
    public static void AddToMainMenu(BuilderDelegate builderDelegate) => mainMenuBuilderDelegate += builderDelegate;

    /// <summary>
    /// Add element(s) to Pause Menu
    /// </summary>
    /// <param name="builderDelegate"></param>
    public static void AddToPauseMenu(BuilderDelegate builderDelegate) => pauseMenuBuilderDelegate += builderDelegate;

    /// <summary>
    /// Creates a page to store your elements
    /// </summary>
    /// <param name="pageName">Name of the GameObject</param>
    /// <returns></returns>
    public static PeakCustomPage CreatePage(string pageName)
    {
        var page = new GameObject(pageName, typeof(PeakCustomPage));

        return page.GetComponent<PeakCustomPage>();
    }

    /// <summary>
    /// Creates a Menu button
    /// </summary>
    /// <param name="buttonName">Text for the button</param>
    /// <returns></returns>
    public static PeakMenuButton? CreateMenuButton(string buttonName)
    {
        if (Templates.ButtonTemplate == null)
        {
            UIPlugin.Log.LogError("You're creating MenuButton too early! Prefab hasn't been loaded yet.");

            return null;
        }

        var clone = Object.Instantiate(Templates.ButtonTemplate);
        clone.name = $"UI_MainMenuButton_{buttonName}";

        var newButton = clone.AddComponent<PeakMenuButton>();

        return newButton.SetText(buttonName);

    }

    /// <summary>
    /// Creates a text label
    /// </summary>
    /// <param name="displayText">Text to display</param>
    /// <returns></returns>
    public static PeakText CreateText(string displayText)
    {
        var gameObj = new GameObject("UI_PeakText", typeof(PeakText));

        return gameObj.GetComponent<PeakText>().SetText(displayText);
    }

    /// <summary>
    /// Creates a text label
    /// </summary>
    /// <param name="displayText">Text to display</param>
    /// <param name="objectName">Name for the <see cref="GameObject"/> (<b>optional</b>)</param>
    /// <returns></returns>
    public static PeakText CreateText(string displayText, string objectName = "UI_PeakText")
    {
        var gameObj = new GameObject(objectName, typeof(PeakText));

        return gameObj.GetComponent<PeakText>().SetText(displayText);
    }

    /// <summary>
    /// The width of the buttons in the Pause Menu
    /// </summary>
    public const float OPTIONS_WIDTH = 277f;
    
    /// <summary>
    /// Same as <see cref="CreateMenuButton(string)"/> but automatically set the <b>width</b> to 277 (<see cref="OPTIONS_WIDTH"/>)
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns></returns>
    public static PeakMenuButton? CreatePauseMenuButton(string buttonName) => CreateMenuButton(buttonName)?.SetWidth(OPTIONS_WIDTH);

}
