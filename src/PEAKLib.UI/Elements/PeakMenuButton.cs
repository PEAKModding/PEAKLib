using PEAKLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Use <see cref="MenuAPI.CreateMenuButton(string)"/>
/// </summary>
public class PeakMenuButton : PeakElement
{
    /// <summary>
    /// The button component
    /// </summary>
    public Button Button { get; private set; } = null!;

    /// <summary>
    /// Button background
    /// </summary>
    public Image Panel { get; private set; } = null!;

    /// <summary>
    /// Button dotted lines (top)
    /// </summary>
    public Image BorderTop { get; private set; } = null!;

    /// <summary>
    /// Button dotted lines (bottom)
    /// </summary>
    public Image BorderBottom { get; private set; } = null!;

    /// <summary>
    /// Button text
    /// </summary>
    public TextMeshProUGUI Text { get; private set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        Button = GetComponent<Button>();
        Panel = transform.Find("Panel").GetComponent<Image>();
        Text = transform.Find("Text").GetComponent<TextMeshProUGUI>();

        // There are two objects named "Border" so we need to do this hack to get the bottom one
        BorderTop = transform.Find("Border").GetComponent<Image>();
        BorderBottom = transform
            .GetChild(BorderTop.transform.GetSiblingIndex() + 1)
            .GetComponent<Image>();
    }

    /// <summary>
    /// Set the button text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public PeakMenuButton SetText(string text)
    {
        Text.text = text;

        return this;
    }

    /// <summary>
    /// Set the button background color
    /// </summary>
    /// <param name="color">Desired color</param>
    /// <param name="automaticBorderColor">If <b>true</b> will automatically calls <see cref="SetBorderColor(Color)"/> with a constrasting color</param>
    /// <returns></returns>
    public PeakMenuButton SetColor(Color color, bool automaticBorderColor = true)
    {
        ThrowHelper.ThrowIfFieldNull(Panel).color = color;

        if (automaticBorderColor)
            SetBorderColor(Utilities.GetContrastingColor(color));

        return this;
    }

    /// <summary>
    /// Set the button border color (dotted lines)
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public PeakMenuButton SetBorderColor(Color color)
    {
        ThrowHelper.ThrowIfFieldNull(BorderTop).color = color;
        ThrowHelper.ThrowIfFieldNull(BorderBottom).color = color;

        return this;
    }

    /// <summary>
    /// Set a custom width for the button, use <see cref="MenuAPI.OPTIONS_WIDTH"/> to be the same width as pause menu buttons
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    public PeakMenuButton SetWidth(float width)
    {
        ThrowHelper.ThrowIfFieldNull(RectTransform);

        RectTransform.sizeDelta = new Vector2(width, RectTransform.sizeDelta.y);

        return this;
    }

    /// <summary>
    /// Same as Button.onClick.AddListener(UnityAction)
    /// </summary>
    /// <param name="onClickEvent"></param>
    /// <returns></returns>
    public PeakMenuButton OnClick(UnityAction onClickEvent)
    {
        ThrowHelper.ThrowIfArgumentNull(onClickEvent);

        Button.onClick.AddListener(onClickEvent);

        return this;
    }
}
