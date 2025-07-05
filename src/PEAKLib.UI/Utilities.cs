using System;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.UI;

/// <summary>
/// Useful functions for UIs
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Get a contrasting color for the target input
    /// </summary>
    /// <param name="input"></param>
    /// <param name="shiftAmount"></param>
    /// <returns></returns>
    public static Color GetContrastingColor(Color input, float shiftAmount = 0.2f)
    {
        Color.RGBToHSV(input, out float h, out float s, out float v);

        if (v > 0.5f)
            v = Mathf.Clamp01(v - shiftAmount);
        else
            v = Mathf.Clamp01(v + shiftAmount);

        return Color.HSVToRGB(h, s, v);
    }

    public static void ExpandToParent(RectTransform target)
    {
        ThrowHelper.ThrowIfArgumentNull(target);

        if (target.parent == null)
        {
            throw new NullReferenceException(
                $"field {nameof(target)}.{nameof(target.parent)} is null!"
            );
        }

        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}
