using UnityEngine;
using UnityEngine.UI;

namespace PEAKLib.UI.Elements;

/// <summary>
/// An element that contains a ScrollRect, parent elements to <see cref="Content"/> to use
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RectMask2D))]
[RequireComponent(typeof(ScrollRect))]
public class PeakScrollableContent : PeakElement
{
    /// <summary>
    /// The content transform, a vertical layout with scrolling
    /// </summary>
    public RectTransform Content { get; private set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        var scrollRect = GetComponent<ScrollRect>();
        scrollRect.scrollSensitivity = 50;
        scrollRect.elasticity = 0;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var contentObj = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter)
        );

        Content = contentObj.GetComponent<RectTransform>();
        Content.SetParent(transform, false);
        Content.pivot = new Vector2(0.5f, 1);

        var layoutGroup = contentObj.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandHeight = false;
        Utilities.ExpandToParent(Content);

        var fitter = contentObj.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = Content;
    }
}
