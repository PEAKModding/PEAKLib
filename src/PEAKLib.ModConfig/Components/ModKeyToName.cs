using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace PEAKLib.ModConfig.Components;

internal class ModKeyToName(ConfigEntryBase key, string name)
{
    internal ConfigEntryBase KeyBind = key;
    internal string ModName = name;

    internal static List<ModKeyToName> RemoveKey(List<ModKeyToName> keys, ConfigEntryBase key)
    {
        ModKeyToName item = keys.FirstOrDefault(i => i.KeyBind == key);
        if (item != null)
            keys.Remove(item);

        return keys;
    }
}
