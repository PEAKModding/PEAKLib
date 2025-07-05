using UnityEngine;
using UnityEngine.UI;
using Zorro.UI;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Horizontal Tabs to use with <see cref="PeakTabContent"/>.<br></br>
/// Needs your own implementation of <see cref="TABS{ButtonType}"/>
/// </summary>
[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectMask2D))]
public class PeakHorizontalTabs : PeakElement
{
    private RectTransform Content { get; set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        RectTransform.anchoredPosition = new Vector2(0, 0);
        RectTransform.sizeDelta = new Vector2(0, 40);
        RectTransform.pivot = new Vector2(0.5f, 1);
        RectTransform.anchorMin = new Vector2(0, 1);
        RectTransform.anchorMax = new Vector2(1, 1);

        var scrollRect = GetComponent<ScrollRect>();

        var contentObj = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup),
            typeof(ContentSizeFitter)
        );
        Content = contentObj.GetComponent<RectTransform>();
        Content.SetParent(transform, false);
        Content.pivot = Vector2.zero;
        Utilities.ExpandToParent(Content);

        scrollRect.content = Content;
        scrollRect.scrollSensitivity = 50;
        scrollRect.elasticity = 0;
        scrollRect.vertical = false;
        scrollRect.horizontal = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var layoutGroup = Content.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.childControlHeight = true;

        var contentSizeFitter = Content.GetComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    /// <summary>
    /// Adds a tab to the Horizontal Tabs. Needs to add your own component implementation of <see cref="TAB_Button"/>.
    /// </summary>
    /// <param name="tabName"></param>
    /// <returns></returns>
    public GameObject AddTab(string tabName)
    {
        var gameObject = new GameObject(
            tabName,
            typeof(RectTransform),
            typeof(Button),
            typeof(LayoutElement)
        );
        gameObject.transform.SetParent(Content, false);

        var layout = gameObject.GetComponent<LayoutElement>();
        layout.minWidth = 220;
        layout.flexibleWidth = 1;
        layout.preferredHeight = 40;

        var imageTransform = new GameObject("Image", typeof(RectTransform), typeof(Image))
            .ParentTo(gameObject.transform)
            .GetComponent<RectTransform>();

        Utilities.ExpandToParent(imageTransform);

        var image = imageTransform.GetComponent<Image>();
        image.color = new Color(0.1792453f, 0.1253449f, 0.09046815f, 0.7294118f);

        var selected = new GameObject("Selected", typeof(RectTransform), typeof(Image))
            .ParentTo(gameObject.transform)
            .GetComponent<RectTransform>();

        Utilities.ExpandToParent(selected);

        var peakText = new GameObject("Text (TMP)", typeof(PeakText))
            .GetComponent<PeakText>()
            .ParentTo(gameObject.transform);

        var textRect = peakText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        peakText.TextMesh.enableAutoSizing = true;
        peakText.TextMesh.fontSizeMin = 18;
        peakText.TextMesh.fontSizeMax = 22;
        peakText.TextMesh.fontSize = 22;
        peakText.TextMesh.fontStyle = TMPro.FontStyles.UpperCase;
        peakText.TextMesh.alignment = TMPro.TextAlignmentOptions.Center;
        peakText.TextMesh.text = tabName;

        return gameObject;
    }
}
