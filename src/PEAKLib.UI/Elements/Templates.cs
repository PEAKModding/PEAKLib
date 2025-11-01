using TMPro;
using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// UI Prefab references to use as templates to instantiate.
/// </summary>
public static class Templates
{
    /// <summary>
    /// Reference to PEAK back button, used as a template for buttons.
    /// Reference grabbed at MainMenu.Start
    /// </summary>
    public static GameObject? ButtonTemplate { get; internal set; }

    /// <summary>
    /// Reference to PEAK settings cell prefab.
    /// Used in ModConfig's Mod Settings Page
    /// Reference grabbed at MainMenu.Start
    /// </summary>
    public static GameObject? SettingsCellPrefab { get; internal set; }

    /// <summary>
    /// Reference to PEAK Reset Bind Button prefab.
    /// Used in ModConfig's Mod Controls Page
    /// Reference grabbed at MainMenu.Start
    /// </summary>
    public static GameObject? ResetBindButton { get; internal set; }

    /// <summary>
    /// Reference to PEAK Bind Warning (Controls) Button prefab.
    /// Used in ModConfig's Mod Controls Page
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static GameObject? BindWarningButton { get; internal set; }

    /// <summary>
    /// Reference to PEAK Keyboard Sprite Sheet.
    /// This can be used to display keyboard sprites with a TMP_SpriteAnimator
    /// Used with ModConfig's Mod Controls Page
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static TMP_SpriteAsset? KeyboardSpriteSheet { get; internal set; }

    /// <summary>
    /// Reference to PEAK Xbox Controller Sprite Sheet.
    /// This can be used to display keyboard sprites with a TMP_SpriteAnimator
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static TMP_SpriteAsset? XboxSpriteSheet { get; internal set; }

    /// <summary>
    /// Reference to PEAK PS4 Controller Sprite Sheet.
    /// This can be used to display keyboard sprites with a TMP_SpriteAnimator
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static TMP_SpriteAsset? PS4SpriteSheet { get; internal set; }

    /// <summary>
    /// Reference to PEAK PS5 Controller Sprite Sheet.
    /// This can be used to display keyboard sprites with a TMP_SpriteAnimator
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static TMP_SpriteAsset? PS5SpriteSheet { get; internal set; }

    /// <summary>
    /// Reference to PEAK Switch Controller Sprite Sheet.
    /// This can be used to display keyboard sprites with a TMP_SpriteAnimator
    /// Reference grabbed at PauseMenuControlsPage.Awake
    /// </summary>
    public static TMP_SpriteAsset? SwitchSpriteSheet { get; internal set; }
}
