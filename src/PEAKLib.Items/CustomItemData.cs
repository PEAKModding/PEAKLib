using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;
using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// Utility and extension methods for reading/writing networked item data.
/// </summary>
public static class CustomItemData
{
    internal const DataEntryKey PeakLibModDataKey = (DataEntryKey)42;

    /// <param name="data">Object serialized from Json.</param>
    /// <inheritdoc cref="SetRawModItemData(ItemComponent, ModDefinition, byte[])"/>
    /// <param name="item"></param>
    /// <param name="mod"></param>
    public static bool TryGetModItemDataFromJson<T>(
        this ItemComponent item,
        ModDefinition mod,
        [MaybeNullWhen(false)] out T data
    )
    {
        data = default;

        if (!TryGetRawModItemData(item, mod, out var rawData))
            return false;

        data = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(rawData));
        return data is not null;
    }

    /// <summary>
    /// Gets PeakLib Mod ItemData for a given <see cref="ModDefinition"/>.
    /// </summary>
    /// <param name="rawData">The data as a byte array or null.</param>
    /// <inheritdoc cref="SetRawModItemData(ItemComponent, ModDefinition, byte[])"/>
    /// <param name="item"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    public static bool TryGetRawModItemData(
        this ItemComponent item,
        ModDefinition mod,
        [NotNullWhen(true)] out byte[]? rawData
    )
    {
        ThrowHelper.ThrowIfArgumentNull(item);
        ThrowHelper.ThrowIfArgumentNull(mod);

        var modData = item.GetData<ModItemData>(PeakLibModDataKey);
        return modData.Value.TryGetValue(mod.GetHashCode(), out rawData);
    }

    /// <param name="data">Object to serialize as Json.</param>
    /// <inheritdoc cref="SetRawModItemData(ItemComponent, ModDefinition, byte[])"/>
    /// <param name="item"></param>
    /// <param name="mod"></param>
    public static void SetModItemDataFromJson(
        this ItemComponent item,
        ModDefinition mod,
        object? data
    ) => SetRawModItemData(item, mod, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));

    /// <summary>
    /// Sets PeakLib Mod ItemData for a given <see cref="ModDefinition"/>.
    /// </summary>
    /// <param name="item"><see cref="global::ItemComponent"/> attached to <see cref="UnityEngine.GameObject"/> with an <see cref="global::Item"/></param>
    /// <param name="mod">The <see cref="ModDefinition"/> this data belongs to.</param>
    /// <param name="rawData">The data to set as a byte array.</param>
    /// <returns></returns>
    public static void SetRawModItemData(this ItemComponent item, ModDefinition mod, byte[] rawData)
    {
        ThrowHelper.ThrowIfArgumentNull(item);
        ThrowHelper.ThrowIfArgumentNull(mod);

        var modData = item.GetData<ModItemData>(PeakLibModDataKey);
        modData.Value[mod.GetHashCode()] = rawData;
    }
}
