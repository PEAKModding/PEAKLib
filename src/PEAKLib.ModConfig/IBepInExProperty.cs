using BepInEx.Configuration;

namespace PEAKLib.ModConfig;

internal interface IBepInExProperty
{
    //so we can refresh values from config and add functional section tabs
    internal ConfigEntryBase ConfigBase { get; }

    internal void RefreshValueFromConfig();
}

