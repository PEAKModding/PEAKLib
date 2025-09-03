using System;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal enum ContentType { Vanilla, Custom, Any }

    internal abstract class ExtendedContent : ScriptableObject
    {
        public ExtendedMod? ExtendedMod { get; internal set; }
        public int GameID { get; private set; }
        public ContentType ContentType { get; internal set; } = ContentType.Vanilla;

        [field: SerializeField] public List<ExtendedTag> ContentTags { get; internal set; } = new List<ExtendedTag>();
        public string ModName => ExtendedMod?.ModName ?? string.Empty;
        public string AuthorName => ExtendedMod?.AuthorName ?? string.Empty;
        public string UniqueIdentificationName => $"{AuthorName.ToLowerInvariant()}.{ModName.ToLowerInvariant()}.{name.ToLowerInvariant()}";

        internal abstract void Register(ExtendedMod mod);
        internal virtual void Initialize() { }
        internal virtual void OnBeforeRegistration() { }
        protected virtual void OnGameIDChanged() { }

        internal void SetGameID(int newID)
        {
            GameID = newID;
            OnGameIDChanged();
        }
        public bool TryGetTag(string tag) => ContentTags.Exists(t => string.Equals(t.TagName, tag, StringComparison.OrdinalIgnoreCase));
        public bool TryAddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            if (TryGetTag(tag)) return false;
            var arr = TagManager.CreateOrGetTags(new[] { tag }, ExtendedMod?.ModName ?? string.Empty);
            if (arr.Length > 0)
            {
                ContentTags.Add(arr[0]);
                return true;
            }
            return false;
        }
    }

    internal abstract class ExtendedContent<T> : ExtendedContent where T : UnityEngine.Object
    {
        public T? ContentObject { get; protected set; }
        protected void SetContent(T content) => ContentObject = content;
    }
}
