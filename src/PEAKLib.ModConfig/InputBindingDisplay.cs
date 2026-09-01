using System;
using PEAKLib.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PEAKLib.ModConfig;

internal static class InputBindingDisplay
{
    private const string UnknownSpriteTag = "<sprite=124 tint=1>";

    public static void SetText(PeakText text, KeyCode keyCode)
    {
        SetText(text.TextMesh, keyCode);
        text.RectTransform.sizeDelta = text.TextMesh.GetPreferredValues();
    }

    public static void SetText(PeakText text, string path)
    {
        SetText(text.TextMesh, path);
        text.RectTransform.sizeDelta = text.TextMesh.GetPreferredValues();
    }

    public static void SetText(TMP_Text text, KeyCode keyCode)
    {
        InputSpriteData spriteData = InputSpriteData.Instance;
        string keyName = keyCode.ToString();
        if (keyName.StartsWith("Joystick", StringComparison.Ordinal))
        {
            text.spriteAsset = GetGamepadSpriteAsset(spriteData, "<Gamepad>");
            text.text = keyName;
            return;
        }

        text.spriteAsset = spriteData.keyboardSprites;
        text.text = GetKeyboardSpriteTag(spriteData, keyCode);
    }

    public static void SetText(TMP_Text text, string path)
    {
        InputSpriteData spriteData = InputSpriteData.Instance;
        switch (InputBindingPath.GetDevice(path))
        {
            case InputBindingDevice.Keyboard:
                SetSpriteOrRaw(
                    text,
                    spriteData.keyboardSprites,
                    spriteData.GetSpriteTagFromInputPathKeyboard(path),
                    path
                );
                break;
            case InputBindingDevice.Mouse:
                SetSpriteOrRaw(
                    text,
                    spriteData.keyboardSprites,
                    spriteData.GetSpriteTagFromInputPathKeyboard(path),
                    path
                );
                break;
            case InputBindingDevice.Gamepad:
                SetSpriteOrRaw(
                    text,
                    GetGamepadSpriteAsset(spriteData, path),
                    spriteData.GetSpriteTagFromInputPathGamepad(path),
                    path
                );
                break;
            default:
                SetRaw(text, path);
                break;
        }
    }

    private static void SetSpriteOrRaw(
        TMP_Text text,
        TMP_SpriteAsset spriteAsset,
        string? spriteTag,
        string rawValue
    )
    {
        if (string.IsNullOrEmpty(spriteTag) || spriteTag == UnknownSpriteTag)
        {
            SetRaw(text, rawValue);
            return;
        }

        text.spriteAsset = spriteAsset;
        text.text = spriteTag;
    }

    private static void SetRaw(TMP_Text text, string rawValue)
    {
        text.spriteAsset = null;
        text.text = rawValue;
    }

    private static string GetKeyboardSpriteTag(InputSpriteData spriteData, KeyCode key)
    {
        if (key == KeyCode.Mouse0)
            return "<sprite=109 tint=1>";

        if (key == KeyCode.Mouse1)
            return "<sprite=110 tint=1>";

        if (key == KeyCode.Mouse2)
            return "<sprite=111 tint=1>";

        string search = key.ToString();

        if (
            search.StartsWith("Alpha", StringComparison.Ordinal)
            && int.TryParse(search["Alpha".Length..], out _)
        )
            search = search.Replace("Alpha", "", StringComparison.Ordinal);

        search = search.Replace("Keypad", "numpad", StringComparison.OrdinalIgnoreCase);
        search = search.Replace("Control", "Ctrl", StringComparison.OrdinalIgnoreCase);
        search = search.Replace("Return", "Enter", StringComparison.OrdinalIgnoreCase);

        if (search.Contains("backquote", StringComparison.OrdinalIgnoreCase))
            search = search.ToLowerInvariant();

        search = char.ToLowerInvariant(search[0]) + search[1..];

        return spriteData.inputPathToSpriteTagKeyboard.TryGetValue(search, out string sprite)
            ? sprite
            : key.ToString();
    }

    private static TMPro.TMP_SpriteAsset GetGamepadSpriteAsset(
        InputSpriteData spriteData,
        string path
    )
    {
        Gamepad? gamepad = Gamepad.current;
        string currentDescriptor = $"{gamepad?.layout} {gamepad?.displayName}".ToLowerInvariant();
        string pathDescriptor = path.ToLowerInvariant();

        if (currentDescriptor.Contains("dualsense", StringComparison.Ordinal))
            return spriteData.ps5Sprites;

        if (currentDescriptor.Contains("dualshock", StringComparison.Ordinal))
            return spriteData.ps4Sprites;

        if (currentDescriptor.Contains("switch", StringComparison.Ordinal))
            return spriteData.switchSprites;

        if (pathDescriptor.Contains("dualsense", StringComparison.Ordinal))
            return spriteData.ps5Sprites;

        if (pathDescriptor.Contains("dualshock", StringComparison.Ordinal))
            return spriteData.ps4Sprites;

        if (pathDescriptor.Contains("switch", StringComparison.Ordinal))
            return spriteData.switchSprites;

        return spriteData.xboxSprites;
    }
}
