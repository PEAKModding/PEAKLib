using PEAKLib.Core;
using TMPro;
using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Used for PeakLib.UI.Components that can contain a Text component
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PeakLocalizableElement : PeakElement
{
    /// <summary>
    ///
    /// </summary>
    public TMP_Text Text { get; internal set; } = null!;

    internal string unlocalizedText = "";
    internal LocalizedText? localizedText;

    internal void SetTextInternal(string text)
    {
        ThrowHelper.ThrowIfFieldNull(Text).text = text;
        unlocalizedText = text;
    }
}
