using System;
using PEAKLib.Core;
using PEAKLib.UI.Elements;
using UnityEngine;

namespace PEAKLib.UI;

/// <summary>
/// Class for making our lifes easier
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Parent <paramref name="instance"/> inside <paramref name="transform"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static T ParentTo<T>(this T instance, Transform transform)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    /// <summary>
    /// Parent <paramref name="instance"/> inside <paramref name="component"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static T ParentTo<T>(this T instance, Component component)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(component);
        var transform = ThrowHelper.ThrowIfFieldNull(component.transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    /// <summary>
    /// Parent <paramref name="instance"/> inside <paramref name="transform"/>
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static GameObject ParentTo(this GameObject instance, Transform transform)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    /// <summary>
    /// Parent <paramref name="instance"/> inside <paramref name="component"/>
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static GameObject ParentTo(this GameObject instance, Component component)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(component);
        var transform = ThrowHelper.ThrowIfFieldNull(component.transform);

        instance.transform.SetParent(transform, false);

        return instance;
    }

    /// <summary>
    /// Changes the order of <paramref name="instance"/>, useful to make an element goes up or down in a menu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static T SetSiblingIndex<T>(this T instance, int index)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.transform.SetSiblingIndex(index);

        return instance;
    }

    /// <summary>
    /// Changes the order of <paramref name="instance"/>, useful to make an element goes up or down in a menu
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static GameObject SetSiblingIndex(this GameObject instance, int index)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.transform.SetSiblingIndex(index);

        return instance;
    }

    /// <summary>
    /// Set the <paramref name="instance"/> anchored <paramref name="position"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static T SetPosition<T>(this T instance, Vector2 position)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchoredPosition = position;

        return instance;
    }

    /// <summary>
    /// Define the <paramref name="size"/> of an <paramref name="instance"/> with Width and Height
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static T SetSize<T>(this T instance, Vector2 size)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.sizeDelta = size;

        return instance;
    }

    /// <summary>
    /// Sets the <see cref="RectTransform.offsetMin"/> of <paramref name="instance"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="offsetMin"></param>
    /// <returns></returns>
    public static T SetOffsetMin<T>(this T instance, Vector2 offsetMin)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.offsetMin = offsetMin;

        return instance;
    }

    /// <summary>
    /// Sets the <see cref="RectTransform.offsetMax"/> of <paramref name="instance"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="offsetMax"></param>
    /// <returns></returns>
    public static T SetOffsetMax<T>(this T instance, Vector2 offsetMax)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.offsetMax = offsetMax;

        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="anchorMin"></param>
    /// <returns></returns>
    public static T SetAnchorMin<T>(this T instance, Vector2 anchorMin)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMin = anchorMin;

        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="anchorMax"></param>
    /// <returns></returns>
    public static T SetAnchorMax<T>(this T instance, Vector2 anchorMax)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMax = anchorMax;

        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="anchorValue"></param>
    /// <returns></returns>
    public static T SetAnchorMinMax<T>(this T instance, Vector2 anchorValue)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.anchorMin = anchorValue;
        instance.RectTransform.anchorMax = anchorValue;

        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="pivot"></param>
    /// <returns></returns>
    public static T SetPivot<T>(this T instance, Vector2 pivot)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        instance.RectTransform.pivot = pivot;

        return instance;
    }

    /// <summary>
    /// Align an element to the parent based on <paramref name="alignment"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    public static T AlignToParent<T>(this T instance, UIAlignment alignment)
        where T : PeakElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(alignment);

        Vector2 anchor = alignment switch
        {
            UIAlignment.TopLeft => new Vector2(0, 1),
            UIAlignment.TopCenter => new Vector2(0.5f, 1),
            UIAlignment.TopRight => new Vector2(1, 1),

            UIAlignment.MiddleLeft => new Vector2(0, 0.5f),
            UIAlignment.MiddleCenter => new Vector2(0.5f, 0.5f),
            UIAlignment.MiddleRight => new Vector2(1, 0.5f),

            UIAlignment.BottomLeft => new Vector2(0, 0),
            UIAlignment.BottomCenter => new Vector2(0.5f, 0),
            UIAlignment.BottomRight => new Vector2(1, 0),

            _ => Vector2.zero,
        };

        var rect = instance.RectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = Vector2.zero;

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

    /// <summary>
    /// Expand the element to fill the entire parent
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static GameObject ExpandToParent(this GameObject instance)
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        if (instance.transform is not RectTransform rectTransform)
            throw new System.Exception($"[{instance.name}] does not contain a RectTransform");

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return instance;
    }

    /// <summary>
    /// Add a localized text for the selected language
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="text"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public static T AddLocalization<T>(
        this T instance,
        string text,
        LocalizedText.Language language
    )
        where T : PeakLocalizableElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);

        if (instance.name == "UI_PeakText")
            throw new Exception(
                "You need to use a unique name to use localization. E.g. MenuAPI.CreateText(\"my cool text\", \"mymod_my_unique_name\")"
            );

        var textComponent = ThrowHelper.ThrowIfFieldNull(instance.Text);

        var index = instance.name.ToUpperInvariant();

        var localizedText = instance.localizedText;
        if (localizedText == null)
        {
            localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
            localizedText.index = index;
            localizedText.tmp = instance.Text;
            instance.localizedText = localizedText;
        }

        if (string.IsNullOrEmpty(instance.unlocalizedText))
            instance.unlocalizedText = text;

        MenuAPI.CreateLocalizationInternal(index, text, language);

        // Refresh text with user current language
        textComponent.text = localizedText.GetText();

        return instance;
    }

    /// <summary>
    /// Set the <see cref="LocalizedText.index"/> to a specific value (useful for using translations from the game)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static T SetLocalizationIndex<T>(this T instance, string index)
        where T : PeakLocalizableElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        var textComponent = ThrowHelper.ThrowIfFieldNull(instance.Text);

        var localizedText = instance.localizedText;
        if (localizedText == null)
        {
            localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
            localizedText.tmp = instance.Text;
            instance.localizedText = localizedText;
        }

        localizedText.index = index;
        textComponent.text = localizedText.GetText();

        return instance;
    }

    /// <summary>
    /// <inheritdoc cref="SetLocalizationIndex{T}(T, string)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    public static T SetLocalizationIndex<T>(this T instance, TranslationKey translationKey)
        where T : PeakLocalizableElement
    {
        ThrowHelper.ThrowIfArgumentNull(instance);
        ThrowHelper.ThrowIfArgumentNull(translationKey);

        instance.SetLocalizationIndex(translationKey.Index);

        return instance;
    }
}
