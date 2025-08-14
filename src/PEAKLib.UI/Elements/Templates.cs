using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// UI Prefab templates to instantiate.
/// </summary>
public static class Templates
{
    /// <summary>
    /// Template for a button.
    /// </summary>
    public static GameObject? ButtonTemplate { get; internal set; }

    /// <summary>
    /// Template for a settings cell.
    /// </summary>
    public static GameObject? SettingsCellPrefab { get; internal set; }
}
