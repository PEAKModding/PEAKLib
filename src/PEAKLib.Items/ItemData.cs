using System;
using UnityEngine;

namespace PEAKLib.Items;

/// <summary>
/// Utility and extension methods for reading/writing networked item data.
/// </summary>
public static class ItemData
{
    const DataEntryKey PeakLibModDataKey = (DataEntryKey)42;

    /// <summary>
    /// Gets PeakLib Mod ItemData for a given ID.
    /// </summary>
    /// <param name="item"><see cref="global::ItemComponent"/> attached to <see cref="UnityEngine.GameObject"/> with an <see cref="global::Item"/></param>
    /// <param name="ID">Unique int ID used by the mod. Easy way to get one is just to hash the mod ID.</param>
    /// <param name="default">Default value to set and return if no entry exists.</param>
    /// <returns></returns>
    public static byte[] GetModItemData(this ItemComponent item, int ID, byte[] @default)
    {
        var modData = item.GetData<ModItemData>(PeakLibModDataKey);
        if (!modData.Value.ContainsKey(ID))
        {
            modData.Value[ID] = @default;
        }
        return modData.Value[ID];
    }

    /// <summary>
    /// Sets PeakLib Mod ItemData for a given ID.
    /// </summary>
    /// <param name="item"><see cref="global::ItemComponent"/> attached to <see cref="UnityEngine.GameObject"/> with an <see cref="global::Item"/></param>
    /// <param name="ID">Unique int ID used by the mod. Easy way to get one is just to hash the mod ID.</param>
    /// <param name="value">Value to set.</param>
    /// <returns></returns>
    public static void SetModItemData(this ItemComponent item, int ID, byte[] value)
    {
        var modData = item.GetData<ModItemData>(PeakLibModDataKey);
        modData.Value[ID] = value;
    }

    /// <summary>
    /// Converts byte array to hex.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string BytesToHex(byte[] value)
    {
        return BitConverter.ToString(value).Replace("-", " ").ToLower();
    }
}
