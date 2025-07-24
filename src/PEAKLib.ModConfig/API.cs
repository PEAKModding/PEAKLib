using BepInEx;
using BepInEx.Configuration;
using System.Reflection;

namespace PEAKLib.ModConfig
{
    /// <summary>
    /// Public methods for use by external mods.
    /// </summary>
    public class API
    {
        /// <summary>
        /// Call this to reload your config in it's entirety after initialization.
        /// Will catch any config items created after initialization while skipping existing config items.
        /// </summary>
        public static void ReloadMyConfig(PluginInfo plugin)
        {
            string caller = ModConfigPlugin.FixNaming(plugin.Metadata.Name);
            ModConfigPlugin.Log.LogMessage($"{caller} has called ReloadMyConfig() from ModConfig.API!");

            if (!ModConfigPlugin.modSettingsLoaded)
            {
                ModConfigPlugin.Log.LogWarning("Unable to reload config, mod settings have not been initialized!");
                return;
            }

            var entries = ModConfigPlugin.GetModConfigEntries();
            if (!entries.ContainsKey(caller))
            {
                ModConfigPlugin.Log.LogWarning($"{caller} does not have any config items!");
                return;
            }

            ModConfigPlugin.ProcessModEntries(caller, entries[caller]);
        }

        /// <summary>
        /// Call this to add an individual config item after initialization.
        /// </summary>
        public static void AddConfigItem(PluginInfo plugin, ConfigEntryBase configEntry)
        {
            string caller = ModConfigPlugin.FixNaming(plugin.Metadata.Name);
            ModConfigPlugin.Log.LogMessage($"{caller} has called AddConfigItem() from ModConfig.API!");

            if (!ModConfigPlugin.modSettingsLoaded)
            {
                ModConfigPlugin.Log.LogWarning("Unable to add config item, mod settings have not been initialized!");
                return;
            }

            ModConfigPlugin.ProcessModEntries(caller, [configEntry]);
        }

        /// <summary>
        /// Call this to add an array of individual config items after initialization.
        /// </summary>
        public static void AddConfigItem(PluginInfo plugin, ConfigEntryBase[] configEntries)
        {
            string caller = ModConfigPlugin.FixNaming(plugin.Metadata.Name);
            ModConfigPlugin.Log.LogMessage($"{caller} has called AddConfigItem() from ModConfig.API!");

            if (!ModConfigPlugin.modSettingsLoaded)
            {
                ModConfigPlugin.Log.LogWarning("Unable to add config items, mod settings have not been initialized!");
                return;
            }

            ModConfigPlugin.ProcessModEntries(caller, configEntries);
        }
    }
}
