using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PEAKLib.UI.Elements;

/// <summary>
/// Use <see cref="MenuAPI.CreatePage(string)"/>
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class PeakCustomPage : MenuWindow
{
    private Canvas Canvas { get; set; } = null!;
    private CanvasScaler Scaler { get; set; } = null!;

    // Background is optional, so can be null
    private Image? Background { get; set; }

    /// <summary>
    /// <inheritdoc cref="OpenOnStart"/>
    /// </summary>
    public override bool openOnStart => OpenOnStart;

    /// <summary>
    /// <inheritdoc cref="SelectOnOpen"/>
    /// </summary>
    public override bool selectOnOpen => SelectOnOpen;

    /// <summary>
    /// <inheritdoc cref="CloseOnPause"/>
    /// </summary>
    public override bool closeOnPause => CloseOnPause;

    /// <summary>
    /// <inheritdoc cref="CloseOnUICancel"/>
    /// </summary>
    public override bool closeOnUICancel => CloseOnUICancel;

    /// <summary>
    /// <inheritdoc cref="AutoHideOnClose"/>
    /// </summary>
    public override bool autoHideOnClose => AutoHideOnClose;

    /// <summary>
    /// <inheritdoc cref="BlocksPlayerInput"/>
    /// </summary>
    public override bool blocksPlayerInput => BlocksPlayerInput;

    /// <summary>
    /// <inheritdoc cref="ShowCursorWhileOpen"/>
    /// </summary>
    public override bool showCursorWhileOpen => ShowCursorWhileOpen;

    /// <summary>
    /// If true the page will automatically opens once started
    /// </summary>
    public bool OpenOnStart { get; set; } = false;

    /// <summary>
    /// If true the page will automatically focus <see cref="MenuWindow.objectToSelectOnOpen"/> when opened
    /// </summary>
    public bool SelectOnOpen { get; set; } = true;

    /// <summary>
    /// If true will close when you pause the game
    /// </summary>
    public bool CloseOnPause { get; set; } = true;

    /// <summary>
    /// If true will close when you leave the pause
    /// </summary>
    public bool CloseOnUICancel { get; set; } = true;

    /// <summary>
    /// If true will automatically hides itself when closed
    /// </summary>
    public bool AutoHideOnClose { get; set; } = true;

    /// <summary>
    /// If true will prevent screen from moving and game inputs (movement, interaction, etc)
    /// </summary>
    public bool BlocksPlayerInput { get; set; } = true;

    /// <summary>
    /// If true will show the cursor when page is opened
    /// </summary>
    public bool ShowCursorWhileOpen { get; set; } = true;

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas.additionalShaderChannels =
            AdditionalCanvasShaderChannels.Normal
            | AdditionalCanvasShaderChannels.TexCoord1
            | AdditionalCanvasShaderChannels.TexCoord2
            | AdditionalCanvasShaderChannels.TexCoord3
            | AdditionalCanvasShaderChannels.Tangent;

        Canvas.sortingOrder = 1;

        Scaler = GetComponent<CanvasScaler>();
        Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        Scaler.referenceResolution = new Vector2(1920, 1080);
        Scaler.matchWidthOrHeight = 1;
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
    public PeakCustomPage CreateBackground(Color? backgroundColor = null)
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

    /// <summary>
    /// </summary>
    public override void OnClose()
    {
        onCloseAction?.Invoke();
    }

    /// <summary>
    /// </summary>
    public override void OnOpen()
    {
        onOpenAction?.Invoke();
    }

    /// <summary>
    /// Set a custom event that will be called when page closes
    /// </summary>
    /// <param name="onCloseEvent"></param>
    /// <returns></returns>
    public PeakCustomPage SetOnClose(UnityAction onCloseEvent)
    {
        onCloseAction = onCloseEvent;
        return this;
    }

    /// <summary>
    /// Set a custom event that will be called when page opens
    /// </summary>
    /// <param name="onOpenEvent"></param>
    /// <returns></returns>
    public PeakCustomPage SetOnOpen(UnityAction onOpenEvent)
    {
        onOpenAction = onOpenEvent;
        return this;
    }
}
