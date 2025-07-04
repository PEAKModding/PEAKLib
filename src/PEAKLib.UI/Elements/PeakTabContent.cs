using UnityEngine;
using UnityEngine.UI;

namespace PEAKLib.UI.Elements;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RectMask2D))]
[RequireComponent(typeof(ScrollRect))]
public class PeakTabContent : PeakElement
{
    public ScrollRect ScrollRect { get; private set; }
    public RectTransform Content { get; private set; }
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        ScrollRect = GetComponent<ScrollRect>();
        ScrollRect.scrollSensitivity = 50;
        ScrollRect.elasticity = 0;
        ScrollRect.vertical = true;
        ScrollRect.horizontal = false;
        ScrollRect.movementType = ScrollRect.MovementType.Clamped;

        Utilities.ExpandToParent(RectTransform);

        RectTransform.offsetMax = new Vector2(0, -60f);

        var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        Content = contentObj.GetComponent<RectTransform>();
        Content.SetParent(transform, false);
        Content.pivot = new Vector2(0.5f, 1);

        var layoutGroup = contentObj.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandHeight = false;
        Utilities.ExpandToParent(Content);

        var fitter = contentObj.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect.content = Content;
    }
}
