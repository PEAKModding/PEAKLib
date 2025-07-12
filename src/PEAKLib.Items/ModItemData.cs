using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Zorro.Core.Serizalization;

namespace PEAKLib.Items;

/// <summary>
/// Similar to other network-serialized types like <see cref="global::IntItemData"/>, but stores mod-indexed bytes as entries.
/// </summary>
internal class ModItemData : DataEntryValue
{
    /// <summary>
    /// Value for input to serialization and output from deserialization
    /// </summary>
    public Dictionary<int, byte[]> Value = new Dictionary<int, byte[]>();

    /// <inheritdoc/>
    public override void SerializeValue(BinarySerializer serializer)
    {
        // number of entries
        serializer.WriteInt(Value.Count);
        foreach (var pair in Value)
        {
            // mod ID
            serializer.WriteInt(pair.Key);
            // bytes length
            serializer.WriteInt(pair.Value.Length);
            // bytes
            serializer.WriteBytes(pair.Value);
        }
    }

    /// <inheritdoc/>
    public override void DeserializeValue(BinaryDeserializer deserializer)
    {
        int count = deserializer.ReadInt();
        for (int i = 0; i < count; i++)
        {
            int mod = deserializer.ReadInt();
            int len = deserializer.ReadInt();
            Value[mod] = deserializer.ReadBytes(len);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        // output as hexadecimal
        sb.Append($"ModItemData({Value.Count}) {{ ");
        foreach (var pair in Value)
        {
            string hex = ItemData.BytesToHex(pair.Value);
            sb.Append($"{pair.Key}=[{hex}], ");
        }
        sb.Append("}");
        return sb.ToString();
    }
}
