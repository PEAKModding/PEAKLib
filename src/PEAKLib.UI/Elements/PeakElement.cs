using UnityEngine;

namespace PEAKLib.UI.Elements;

/// <summary>
/// The base of all UI elements, specially made to use with <see cref="ElementExtensions"/>
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PeakElement : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public RectTransform RectTransform { get; internal set; } = null!;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}
