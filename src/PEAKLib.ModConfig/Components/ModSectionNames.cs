using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace PEAKLib.ModConfig.Components;

internal class ModSectionNames
{
    internal static List<ModSectionNames> SectionNames { get; set; } = [];
    internal string ModName { get; set; }
    internal List<string> Sections { get; set; } = [];

    internal ModSectionNames(string modName)
    {
        ModName = modName;
        SectionNames.Add(this);
    }

    internal static ModSectionNames SetMod(string modName)
    {
        ModSectionNames sectionTracker = SectionNames.FirstOrDefault(x => x.ModName == modName);
        if (sectionTracker == null!)
            sectionTracker = new(modName);

        return sectionTracker;
    }

    internal static bool TryGetModSections(string modName, out List<string> sections)
    {
        sections = [];
        ModSectionNames sectionTracker = SectionNames.FirstOrDefault(x => x.ModName == modName);
        if (sectionTracker == null)
            return false;

        sections = sectionTracker.Sections;
        return sections.Count > 0;
    }

    internal static bool TryGetFirstSection(string modName, out string section)
    {
        section = string.Empty;
        ModSectionNames sectionTracker = SectionNames.FirstOrDefault(x => x.ModName == modName);
        if (sectionTracker == null)
            return false;

        section = sectionTracker.Sections[0];
        return true;
    }

    internal void CheckSectionName(string sectionName)
    {
        if (!Sections.Contains(sectionName))
            Sections.Add(sectionName);
    }
}
