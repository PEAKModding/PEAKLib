using PEAKLib.Core;
using PEAKLib.UI.Elements;
using UnityEngine;

namespace PEAKLib.UI;

/// <summary>
/// Main API to create everything related to UI
/// </summary>
public static class MenuAPI
{
    internal static BuilderDelegate? pauseMenuBuilderDelegate,
        mainMenuBuilderDelegate,
        settingsMenuBuilderDelegate;

    /// <summary>
    /// Delegate to create elements
    /// </summary>
    /// <param name="transform">The target transform</param>
    public delegate void BuilderDelegate(Transform transform);

    /// <summary>
    /// Add element(s) to Main Menu
    /// </summary>
    /// <param name="builderDelegate"></param>
    public static void AddToMainMenu(BuilderDelegate builderDelegate) =>
        mainMenuBuilderDelegate += builderDelegate;

    /// <summary>
    /// Add element(s) to Pause Menu
    /// </summary>
    /// <param name="builderDelegate"></param>
    public static void AddToPauseMenu(BuilderDelegate builderDelegate) =>
        pauseMenuBuilderDelegate += builderDelegate;

    /// <summary>
    /// Add element(s) to Setting Menu
    /// </summary>
    /// <param name="builderDelegate"></param>
    public static void AddToSettingsMenu(BuilderDelegate builderDelegate) =>
        settingsMenuBuilderDelegate += builderDelegate;

    /// <summary>
    /// Creates a page to store your elements
    /// </summary>
    /// <param name="pageName">Name of the GameObject</param>
    /// <returns></returns>
    public static PeakCustomPage CreatePage(string pageName)
    {
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(pageName);

        return new GameObject(pageName).AddComponent<PeakCustomPage>();
    }

    /// <summary>
    /// Creates a page to store your elements
    /// </summary>
    /// <param name="pageName">Name of the GameObject</param>
    /// <returns></returns>
    public static PeakCustomPage CreatePageWithBackground(string pageName) =>
        CreatePage(pageName).CreateBackground();
    

    /// <summary>
    /// Creates a Menu button
    /// </summary>
    /// <param name="buttonName">Text for the button</param>
    /// <returns></returns>
    public static PeakMenuButton CreateMenuButton(string buttonName)
    {
        ThrowHelper.ThrowIfFieldNull(buttonName);

        if (Templates.ButtonTemplate == null)
            throw new System.Exception(
                "You're creating MenuButton too early! Prefab hasn't been loaded yet."
            );

        var clone = Object.Instantiate(Templates.ButtonTemplate);
        clone.name = $"UI_MainMenuButton_{buttonName}";

        var newButton = clone.AddComponent<PeakMenuButton>();

        return newButton.SetText(buttonName);
    }

    /// <summary>
    /// Same as <see cref="CreateMenuButton(string)"/> but automatically set the <b>width</b> to 277 (<see cref="OPTIONS_WIDTH"/>)
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns></returns>
    public static PeakMenuButton CreatePauseMenuButton(string buttonName) =>
        CreateMenuButton(buttonName).SetWidth(OPTIONS_WIDTH);

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
        ThrowHelper.ThrowIfArgumentNull(displayText);
        ThrowHelper.ThrowIfArgumentNullOrWhiteSpace(objectName);

        var gameObj = new GameObject(objectName, typeof(PeakText));

        return gameObj.GetComponent<PeakText>().SetText(displayText);
    }

    /// <summary>
    /// The width of the buttons in the Pause Menu
    /// </summary>
    public const float OPTIONS_WIDTH = 277f;

    /// <summary>
    /// Creates a simple button without styling
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns></returns>
    public static PeakButton CreateButton(string buttonName)
    {
        ThrowHelper.ThrowIfFieldNullOrWhiteSpace(buttonName);

        var gameObj = new GameObject(buttonName);

        return gameObj.AddComponent<PeakButton>();
    }

    /// <summary>
    /// Create a <see cref="PeakScrollableContent"/>, parent things to <see cref="PeakScrollableContent.Content"/> to use
    /// </summary>
    /// <param name="scrollableName"></param>
    /// <returns></returns>
    public static PeakScrollableContent CreateScrollableContent(string scrollableName) =>
        new GameObject(scrollableName).AddComponent<PeakScrollableContent>();

    /// <summary>
    /// <inheritdoc cref="PeakTextInput"/>
    /// </summary>
    /// <param name="inputName"></param>
    /// <returns></returns>
    public static PeakTextInput CreateTextInput(string inputName)
    {
        var textInput = PeakTextInput.Create();
        textInput.name = inputName;

        return textInput;
    }
    
}
