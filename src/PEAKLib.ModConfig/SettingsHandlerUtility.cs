using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using PEAKLib.ModConfig.SettingOptions;
using UnityEngine;

namespace PEAKLib.ModConfig;

internal static class SettingsHandlerUtility
{
    internal static void AddBoolToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<bool>? saveCallback = null
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExOffOn(entry, tabName, saveCallback));
    }

    internal static void AddFloatToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<float>? applyCallback = null
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExFloat(entry, tabName, applyCallback));
    }

    internal static void AddDoubleToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<double>? applyCallback = null
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExDouble(entry, tabName, applyCallback));
    }

    internal static void AddIntToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<int>? saveCallback = null
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExInt(entry, tabName, saveCallback));
    }

    internal static void AddStringToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<string>? saveCallback = null
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExString(entry, tabName, saveCallback));
    }

    internal static void AddKeybindToTab(
        ConfigEntryBase entry,
        string tabName,
        Action<KeyCode>? saveCallback
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExKeyCode(entry, tabName, saveCallback));
    }

    internal static void AddEnumToTab(
        ConfigEntryBase entry,
        string tabName,
        bool isEnum,
        Action<string>? saveCallback
    )
    {
        if (SettingsHandler.Instance == null)
            throw new Exception(
                "You're registering options too early! Use the Start() function to create new options!"
            );

        SettingsHandler.Instance.AddSetting(new BepInExEnum(entry, tabName, isEnum, saveCallback));
    }

    public static T GetDefaultValue<T>(ConfigEntry<T> entry)
    {
        return (T)entry.DefaultValue;
    }

    public static T GetDefaultValue<T>(ConfigEntryBase entry)
    {
        return (T)entry.DefaultValue;
    }

    public static T GetCurrentValue<T>(ConfigEntryBase entry)
    {
        return (T)entry.BoxedValue;
    }

    public static List<T> GetAcceptableValues<T>(ConfigEntryBase entry)
        where T : IEquatable<T>
    {
        if (entry.Description.AcceptableValues is AcceptableValueList<T> valueList)
        {
            return [.. valueList.AcceptableValues];
        }
        else
            return [];
    }

    public static bool TryGetMinMaxValue<T>(ConfigEntryBase entry, out T minValue, out T maxValue)
        where T : IComparable
    {
        minValue = default!;
        maxValue = default!;
        if (entry.Description.AcceptableValues is AcceptableValueRange<T> range)
        {
            minValue = range.MinValue;
            maxValue = range.MaxValue;
            return true;
        }

        return false;
    }
}
