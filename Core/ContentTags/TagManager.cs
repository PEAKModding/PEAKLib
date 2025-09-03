using System;
using System.Collections.Generic;
using System.Linq;
using PEAKLib.Levels.Core;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal static class TagManager
    {
        internal static Dictionary<string, List<ExtendedTag>> globalTagDictionary = new Dictionary<string, List<ExtendedTag>>(StringComparer.OrdinalIgnoreCase);
        internal static Dictionary<string, List<ExtendedContent>> globalTagToExtendedContent = new Dictionary<string, List<ExtendedContent>>(StringComparer.OrdinalIgnoreCase);

        public static void MergeExtendedModTags(ExtendedMod ExtendedMod)
        {
            if (ExtendedMod == null) return;

            var localCanon = new Dictionary<string, ExtendedTag>(StringComparer.OrdinalIgnoreCase);
            foreach (var ext in ExtendedMod.ExtendedContents)
            {
                for (int i = ext.ContentTags.Count - 1; i >= 0; i--)
                {
                    var tag = ext.ContentTags[i];
                    if (tag == null) continue;
                    if (localCanon.TryGetValue(tag.TagName, out var existing))
                    {
                        ext.ContentTags[i] = existing;
                    }
                    else
                    {
                        localCanon[tag.TagName] = tag;
                    }
                }
            }
        }
        public static void MergeAllExtendedModTags()
        {
            var globalCanon = new Dictionary<string, ExtendedTag>(StringComparer.OrdinalIgnoreCase);
            foreach (var mod in PatchedContent.ExtendedMods)
            {
                foreach (var ext in mod.ExtendedContents)
                {
                    for (int i = 0; i < ext.ContentTags.Count; i++)
                    {
                        var tag = ext.ContentTags[i];
                        if (tag == null) continue;
                        if (globalCanon.TryGetValue(tag.TagName, out var existing))
                        {
                            ext.ContentTags[i] = existing;
                        }
                        else
                        {
                            globalCanon[tag.TagName] = tag;
                        }
                    }
                }
            }
        }

        internal static void PopulateTagData()
        {
            globalTagDictionary.Clear();
            globalTagToExtendedContent.Clear();

            var allTags = new List<ExtendedTag>();
            foreach (var mod in PatchedContent.ExtendedMods)
            {
                foreach (var extension in mod.ExtendedContents)
                {
                    foreach (var tag in extension.ContentTags)
                    {
                        if (tag == null) continue;
                        if (!allTags.Contains(tag))
                            allTags.Add(tag);
                    }
                }
            }
            foreach (var tag in allTags)
            {
                if (!globalTagDictionary.TryGetValue(tag.TagName!, out var list))
                {
                    list = new List<ExtendedTag>();
                    globalTagDictionary[tag.TagName!] = list;
                }
                list.Add(tag);
            }
            foreach (var mod in PatchedContent.ExtendedMods)
            {
                foreach (var ext in mod.ExtendedContents)
                {
                    foreach (var tag in ext.ContentTags)
                    {
                        if (tag == null) continue;
                        if (!globalTagToExtendedContent.TryGetValue(tag.TagName!, out var extList))
                        {
                            extList = new List<ExtendedContent>();
                            globalTagToExtendedContent[tag.TagName!] = extList;
                        }
                        if (!extList.Contains(ext)) extList.Add(ext);
                    }
                }
            }
        }

        public static List<ExtendedContent> GetAllExtendedContentsByTag(string Tag)
        {
            if (string.IsNullOrEmpty(Tag)) return new List<ExtendedContent>();
            if (globalTagToExtendedContent.TryGetValue(Tag, out var list))
                return new List<ExtendedContent>(list);
            return new List<ExtendedContent>();
        }
        public static ExtendedTag[] CreateOrGetTags(IEnumerable<string> TagNames, string SourceMod = "")
        {
            if (TagNames == null) return Array.Empty<ExtendedTag>();
            var Result = new List<ExtendedTag>();

            foreach (var Tag in TagNames)
            {
                if (string.IsNullOrWhiteSpace(Tag)) continue;
                ExtendedTag tag = GetOrCreateCanonicalTag(Tag.Trim(), SourceMod);
                if (tag != null) Result.Add(tag);
            }

            return Result.ToArray();
        }
        internal static ExtendedTag GetOrCreateCanonicalTag(string TagName, string SourceMod = "")
        {
            if (string.IsNullOrEmpty(TagName)) return null!;
            if (globalTagDictionary.TryGetValue(TagName, out var list) && list.Count > 0)
            {
                return list[0];
            }

            var NewTag = ExtendedTag.Create(TagName, null, SourceMod);
            globalTagDictionary[TagName] = new List<ExtendedTag> { NewTag };
            return NewTag;
        }
    }
}
