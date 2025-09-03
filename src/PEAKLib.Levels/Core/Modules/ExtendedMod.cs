using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    [CreateAssetMenu(fileName = "ExtendedMod", menuName = "PEAK/ExtendedMod", order = 10)]
    internal class ExtendedMod : ScriptableObject
    {
        [field: SerializeField] public string ModName { get; internal set; } = "Unspecified";
        [field: SerializeField] public string AuthorName { get; internal set; } = "Unknown";
        [field: SerializeField] public string Version { get; internal set; } = "0.0.1"; // Compatiblity version
        [field: SerializeField] public List<string> ModNameAliases { get; internal set; } = new List<string>();
        [field: SerializeField] public List<string> StreamingBundleNames { get; private set; } = new List<string>();
        [field: SerializeField] public List<ExtendedContent> ExtendedContents { get; private set; } = new List<ExtendedContent>();

        public static ExtendedMod CreateNewMod(string? ModName = null, string? AuthorName = null, string? Version = null, params ExtendedContent[] Contents)
        {
            ExtendedMod NewMod = CreateInstance<ExtendedMod>();
            if (!string.IsNullOrEmpty(ModName)) NewMod.ModName = ModName;
            if (!string.IsNullOrEmpty(AuthorName)) NewMod.AuthorName = AuthorName;
            if (!string.IsNullOrEmpty(Version)) NewMod.Version = Version;
            NewMod.name = (ModName ?? "UnnamedMod").Replace(" ", "_") + "_Mod";
            if (Contents != null && Contents.Length > 0) NewMod.TryRegisterExtendedContents(Contents);
            return NewMod;
        }

        public void TryRegisterExtendedContents(params ExtendedContent[] Contents)
        {
            if (Contents == null) return;
            foreach (var Content in Contents) TryRegisterExtendedContent(Content);
        }
        public void TryRegisterExtendedContent(ExtendedContent Content)
        {
            if (Content == null) return;
            try
            {
                if (ExtendedContents.Contains(Content))
                {
                    Debug.LogWarning($"ExtendedMod: Content {Content.name} already registered in {ModName}"); return;
                }
                Content.Register(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        internal void RegisterExtendedContentInternal(ExtendedContent Content)
        {
            if (Content == null) return;
            Content.ExtendedMod = this;
            if (Content.ContentType == ContentType.Custom && (Content.ContentTags == null || Content.ContentTags.Count == 0))
            {
                var CustomTags = TagManager.CreateOrGetTags(new[] { "Custom" }, this.ModName);
                foreach (var tag in CustomTags)
                {
                    if (!(tag == null && Content.ContentTags!.Contains(tag!))) Content.ContentTags!.Add(tag!);
                }
            }

            ExtendedContents.Add(Content);
        }

        internal void UnregisterAll()
        {
            ExtendedContents.Clear();
        }
        internal void SortRegisteredContent()
        {
            ExtendedContents.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
