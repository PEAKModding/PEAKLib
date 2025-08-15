﻿using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// UI Prefab references to use as templates to instantiate.
/// </summary>
public static class Templates
{
    /// <summary>
    /// Reference to PEAK back button, used as a template for buttons.
    /// </summary>
    public static GameObject? ButtonTemplate { get; internal set; }

    /// <summary>
    /// Reference to PEAK settings cell prefab.
    /// </summary>
    public static GameObject? SettingsCellPrefab { get; internal set; }
}
