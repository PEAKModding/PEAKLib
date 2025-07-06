using UnityEngine.UI;
using UnityEngine;
using PEAKLib.Core;
using UnityEngine.Events;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Default Button Non-Stylized 
/// </summary>
[RequireComponent(typeof(Button))]
public class PeakButton : PeakElement
{
    /// <summary>
    /// Button component
    /// </summary>
    public Button Button { get; set; } = null!;

    /// <summary>
    /// <inheritdoc cref="PeakText"/>
    /// </summary>
    public PeakText Text { get; set; } = null!;


    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
      
        var image = new GameObject("Image", typeof(Image))
            .ParentTo(transform)
            .ExpandToParent()
            .GetComponent<Image>();

        image.color = new Color(0.1792453f, 0.1253449f, 0.09046815f, 0.7294118f);

        Text = MenuAPI.CreateText(name, "Text (TMP)")
            .ParentTo(transform)
            .ExpandToParent()
            .SetPosition(Vector2.zero);

        Button = GetComponent<Button>();
        Button.image = image;

        var textMesh = Text.TextMesh;
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = 18;
        textMesh.fontSizeMax = 22;
        textMesh.fontSize = 22;
        textMesh.fontStyle = TMPro.FontStyles.UpperCase;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
    }

    /// <summary>
    /// Set a custom width for the button
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    public PeakButton SetWidth(float width)
    {
        ThrowHelper.ThrowIfFieldNull(RectTransform);

        RectTransform.sizeDelta = new Vector2(width, RectTransform.sizeDelta.y);

        return this;
    }

    /// <summary>
    /// Set a custom height for the button
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public PeakButton SetHeight(float height)
    {
        ThrowHelper.ThrowIfFieldNull(RectTransform);

        RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);

        return this;
    }

    /// <summary>
    /// Same as Button.onClick.AddListener(UnityAction)
    /// </summary>
    /// <param name="onClickEvent"></param>
    /// <returns></returns>
    public PeakButton OnClick(UnityAction onClickEvent)
    {
        ThrowHelper.ThrowIfArgumentNull(onClickEvent);

        Button.onClick.AddListener(onClickEvent);

        return this;
    }
}
