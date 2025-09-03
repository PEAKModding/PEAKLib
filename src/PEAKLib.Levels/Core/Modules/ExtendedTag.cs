using UnityEngine;

namespace PEAKLib.Levels.Core
{
    [CreateAssetMenu(fileName = "ExtendedTag", menuName = "PEAK/ExtendedTag", order = 21)]
    internal class ExtendedTag : ScriptableObject
    {
        [Tooltip("Unique tag name (case-insensitive)")]
        public string TagName = string.Empty;
        [Tooltip("Optional color for editor/UI")]
        public Color TagColor = Color.white;
        [Tooltip("Optional description for tooling")]
        [TextArea(2, 6)]
        public string Description = string.Empty;
        [Tooltip("Source mod id (optional)")]
        public string SourceMod = string.Empty;

        public static ExtendedTag Create(string? name = null, Color? color = null, string Source = "")
        {
            var tag = CreateInstance<ExtendedTag>();
            tag.TagName = name ?? string.Empty;
            tag.TagColor = color ?? Color.white;
            tag.SourceMod = Source ?? string.Empty;
            tag.name = (name ?? "Unnamed") + "_ExtendedTag";
            return tag;
        }
    }

    internal class ModContentTagMarker : MonoBehaviour
    {
        public ExtendedTag[]? tags;
    }
}
