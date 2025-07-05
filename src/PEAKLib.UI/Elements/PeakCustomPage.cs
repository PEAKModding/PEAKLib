using UnityEngine;
using UnityEngine.UI;

namespace PEAKLib.UI.Elements;

// TODO: Need to add summary for the booleans

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

    public override bool openOnStart => OpenOnStart;
    public override bool selectOnOpen => SelectOnOpen;
    public override bool closeOnPause => CloseOnPause;
    public override bool closeOnUICancel => CloseOnUICancel;
    public override bool autoHideOnClose => AutoHideOnClose;
    public override bool blocksPlayerInput => BlocksPlayerInput;
    public override bool showCursorWhileOpen => ShowCursorWhileOpen;

    public bool OpenOnStart { get; set; } = false;
    public bool SelectOnOpen { get; set; } = true;
    public bool CloseOnPause { get; set; } = true;
    public bool CloseOnUICancel { get; set; } = true;
    public bool AutoHideOnClose { get; set; } = true;
    public bool BlocksPlayerInput { get; set; } = true;
    public bool ShowCursorWhileOpen { get; set; } = true;

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3 | AdditionalCanvasShaderChannels.Tangent;

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
}