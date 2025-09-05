using System.Collections.Generic;

namespace PEAKLib.Levels.Core
{
    internal static class PatchedContent
    {
        // public static List<ExtendedMod> ExtendedMods { get; set; } = new List<ExtendedMod>();
        /* public static void SortExtendedMods()
        {
            ExtendedMods.Sort((a, b) => string.Compare(a.ModName, b.ModName, System.StringComparison.OrdinalIgnoreCase));
            foreach (var m in ExtendedMods) m.SortRegisteredContent();
        }

        public static Dictionary<string, List<ExtendedTag>> ModDefinedTags { get; internal set; }
            = new Dictionary<string, List<ExtendedTag>>(); */

        public class SpawnableEntry
        {
            public string BundlePath;
            public string PrefabName;
            public SpawnableEntry(string bundlePath, string prefabName) { this.BundlePath = bundlePath; this.PrefabName = prefabName; }
        }

        public static class SpawnableRegistry
        {
            public static Dictionary<string, SpawnableEntry> Registry = new Dictionary<string, SpawnableEntry>(System.StringComparer.OrdinalIgnoreCase);
        }
    }
}
