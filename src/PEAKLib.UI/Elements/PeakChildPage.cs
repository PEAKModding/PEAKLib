using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zorro.UI;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Class that can be used to create pages that have a parent (back button).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PeakChildPage : UIPage, IHaveParentPage
{
    /// <summary>
    /// The back button of this UI page that transitions to <see cref="ParentPage"/>.
    /// </summary>
    public Button? BackButton { get; set; }

    /// <summary>
    /// The parent page of this UI page.
    /// </summary>
    public UIPage? ParentPage { get; set; }

    // Background is optional, so can be null
    private Image? Background { get; set; }

    private void Start() => BackButton?.onClick.AddListener(new UnityAction(this.BackClicked));

    private void Awake()
    {
        var rectTransform = GetComponent<RectTransform>();

        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;

        var uiPageHandler =
            gameObject.GetComponentInParent<UIPageHandler>(true)
            ?? throw new System.Exception(
                "UIPageHandler not found in the object parent. Make sure to put your page object inside a UIPageHandler. (e.g. MainMenuPageHandler or PauseMenuHandler)"
            );

        pageHandler = uiPageHandler;
    }

    /// <summary>
    /// Sets a parent page, it is used to transition back to the parent when back button is pressed.
    /// </summary>
    /// <param name="parent">The page that it should go back to</param>
    /// <returns></returns>
    public PeakChildPage SetParentPage(UIPage parent)
    {
        ParentPage = parent;
        return this;
    }

    /// <summary>
    /// Sets the "back" button, it will be used to transition to the parent page.
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public PeakChildPage SetBackButton(Button button)
    {
        BackButton = button;
        return this;
    }

    /// <summary>
    /// Internal function used by PEAK to get the parent page, set a parent using <see cref="SetParentPage(UIPage)"/>.
    /// </summary>
    /// <returns></returns>
    public (UIPage, PageTransistion) GetParentPage()
    {
        if (ParentPage == null)
            throw new System.Exception(
                "You haven't set a parent page. Please set a parent page or the back button will not work properly. | myPage.SetParentPage(parentPage)"
            );

        return (ParentPage, new SetActivePageTransistion());
    }

    private void BackClicked()
    {
        var result = GetParentPage();

        pageHandler.TransistionToPage(result.Item1, result.Item2);
    }

    /// <summary>
    /// The default background color
    /// </summary>
    public static readonly Color DEFAULT_BACKGROUND_COLOR = new(0, 0, 0, 0.9569f);

    /// <summary>
    /// Create a semi-transparent background for the current page
    /// </summary>
    /// <param name="backgroundColor">Set a custom background color (<b>optional</b>)</param>
    /// <returns></returns>
    public PeakChildPage CreateBackground(Color? backgroundColor = null)
    {
        if (Background == null)
        {
            var newBackground = new GameObject("Background", typeof(CanvasRenderer), typeof(Image));
            newBackground.transform.SetParent(transform, false);

            Background = newBackground.GetComponent<Image>();

            var backgroundTransform = Background.rectTransform;
            backgroundTransform.anchorMin = Vector2.zero;
            backgroundTransform.anchorMax = Vector2.one;
            backgroundTransform.offsetMin = backgroundTransform.offsetMax = Vector2.zero;

            if (!backgroundColor.HasValue)
                backgroundColor = DEFAULT_BACKGROUND_COLOR;

            Background.color = backgroundColor.Value;
        }

        return this;
    }

    private UnityAction? onCloseAction,
        onOpenAction;

    private void OnDisable()
    {
        onCloseAction?.Invoke();
    }

    private void OnEnable()
    {
        onOpenAction?.Invoke();
    }

    /// <summary>
    /// Set a custom event that will be called when page closes
    /// </summary>
    /// <param name="onCloseEvent"></param>
    /// <returns></returns>
    public PeakChildPage SetOnClose(UnityAction onCloseEvent)
    {
        onCloseAction = onCloseEvent;
        return this;
    }

    /// <summary>
    /// Set a custom event that will be called when page opens
    /// </summary>
    /// <param name="onOpenEvent"></param>
    /// <returns></returns>
    public PeakChildPage SetOnOpen(UnityAction onOpenEvent)
    {
        onOpenAction = onOpenEvent;
        return this;
    }
}
