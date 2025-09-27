using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PEAKLib.ModConfig.Components;
internal class ModKeyToName(ConfigEntry<KeyCode> key, string name)
{
    internal ConfigEntry<KeyCode> KeyBind = key;
    internal string ModName = name;

    internal static List<ModKeyToName> RemoveKey(List<ModKeyToName> keys, ConfigEntry<KeyCode> key)
    {
        ModKeyToName item = keys.FirstOrDefault(i => i.KeyBind == key);
        if (item != null)
            keys.Remove(item);

        return keys;
    }
}
