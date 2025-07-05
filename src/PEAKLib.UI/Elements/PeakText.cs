using System.Linq;
using PEAKLib.Core;
using TMPro;
using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Use <see cref="MenuAPI.CreateText(string)"/>
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class PeakText : PeakElement
{
    private static TMP_FontAsset? _darumaFontAsset;
    private static TMP_FontAsset DarumaDropOne
    {
        get
        {
            if (_darumaFontAsset == null)
            {
                var assets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                _darumaFontAsset = assets.FirstOrDefault(fontAsset =>
                    fontAsset.faceInfo.familyName == "Daruma Drop One"
                );
            }

            return _darumaFontAsset;
        }
    }

    public TextMeshProUGUI TextMesh { get; private set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        TextMesh = GetComponent<TextMeshProUGUI>();
        RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 1);
        RectTransform.pivot = new Vector2(0, 1);

        TextMesh.font = DarumaDropOne;
        TextMesh.color = Color.white;
        RectTransform.sizeDelta = TextMesh.GetPreferredValues();
    }

    /// <summary>
    /// Set the display text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public PeakText SetText(string text)
    {
        ThrowHelper.ThrowIfFieldNull(TextMesh).text = text;
        ThrowHelper.ThrowIfFieldNull(RectTransform).sizeDelta = TextMesh.GetPreferredValues();

        return this;
    }

    /// <summary>
    /// Set the font size
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public PeakText SetFontSize(float size)
    {
        ThrowHelper.ThrowIfFieldNull(TextMesh).fontSize = size;
        ThrowHelper.ThrowIfFieldNull(RectTransform).sizeDelta = TextMesh.GetPreferredValues();

        return this;
    }

    /// <summary>
    /// Set the text color
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public PeakText SetColor(Color color)
    {
        ThrowHelper.ThrowIfFieldNull(TextMesh).color = color;

        return this;
    }
}
