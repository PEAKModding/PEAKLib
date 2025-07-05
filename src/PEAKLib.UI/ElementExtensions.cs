using PEAKLib.Core;
using PEAKLib.UI.Elements;
using UnityEngine;

namespace PEAKLib.UI;

/// <summary>
/// Class for making our lifes easier
/// </summary>
public static class ElementExtensions
{
    public static T ParentTo<T>(this T instance, Transform transform)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    public static GameObject ParentTo(this GameObject instance, Transform transform)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    public static T SetSiblingIndex<T>(this T instance, int index)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.transform.SetSiblingIndex(index);

        return instance;
    }

    public static GameObject SetSiblingIndex(this GameObject instance, int index)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.transform.SetSiblingIndex(index);

        return instance;
    }

    public static T SetPosition<T>(this T instance, Vector2 position)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchoredPosition = position;

        return instance;
    }

    public static T SetSize<T>(this T instance, Vector2 size)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.sizeDelta = size;

        return instance;
    }

    public static T SetAnchorMin<T>(this T instance, Vector2 anchorMin)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMin = anchorMin;

        return instance;
    }

    public static T SetAnchorMax<T>(this T instance, Vector2 anchorMax)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMax = anchorMax;

        return instance;
    }

    public static T SetAnchorMinMax<T>(this T instance, Vector2 anchorValue)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMin = anchorValue;
        instance.RectTransform.anchorMax = anchorValue;

        return instance;
    }

    public static T SetPivot<T>(this T instance, Vector2 pivot)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.pivot = pivot;

        return instance;
    }

    /// <summary>
    /// Expand the element to fill the entire parent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static T ExpandToParent<T>(this T instance)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        var rectTransform = instance.RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return instance;
    }
}
