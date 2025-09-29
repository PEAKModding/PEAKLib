using PEAKLib.ModConfig.SettingOptions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.ModConfig;

internal static class SettingsHandlerUtility
{
    internal static void AddBoolToTab(string displayName, bool defaultValue, string tabName, bool currentValue = false, Action<bool>? saveCallback = null)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");

        SettingsHandler.Instance.AddSetting(new BepInExOffOn(displayName, defaultValue, tabName, currentValue, saveCallback));
    }

    internal static void AddFloatToTab(string displayName, float defaultValue,
        string tabName, float minValue = 0f, float maxValue = 1f, float currentValue = 0f, Action<float>? applyCallback = null)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");
   

        SettingsHandler.Instance.AddSetting(new BepInExFloat(displayName, defaultValue, tabName, minValue, maxValue, currentValue, applyCallback));
    }
    internal static void AddDoubleToTab(string displayName, double defaultValue,
       string tabName, double minValue = 0f, double maxValue = 1f, double currentValue = 0f, Action<double>? applyCallback = null)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");


        SettingsHandler.Instance.AddSetting(new BepInExDouble(displayName, defaultValue, tabName, minValue, maxValue, currentValue, applyCallback));
    }

    internal static void AddIntToTab(string displayName, int defaultValue, string tabName, int currentValue = 0, Action<int>? saveCallback = null)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");
           

        SettingsHandler.Instance.AddSetting(new BepInExInt(displayName, tabName, defaultValue, currentValue, saveCallback));
    }

    internal static void AddStringToTab(string displayName, string defaultValue, string tabName, string currentValue = "", Action<string>? saveCallback = null)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");

        SettingsHandler.Instance.AddSetting(new BepInExString(displayName, tabName, defaultValue, currentValue, saveCallback));
    }
    internal static void AddKeybindToTab(string displayName, KeyCode defaultValue, string tabName, KeyCode currentValue, Action<KeyCode>? saveCallback)
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");

        SettingsHandler.Instance.AddSetting(new BepInExKeyCode(displayName, tabName, defaultValue, currentValue, saveCallback));
    }

    internal static void AddEnumToTab(string displayName, List<string> options, string tabName, string currentValue, Action<string>? saveCallback) 
    {
        if (SettingsHandler.Instance == null)
            throw new Exception("You're registering options too early! Use the Start() function to create new options!");

        SettingsHandler.Instance.AddSetting(new BepInExEnum(displayName, options, currentValue, tabName, saveCallback));
    }
}