using System.Collections.Generic;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Levels.UnityEditor;

// TODO: Implement this.

/// <summary>
/// A <see cref="ScriptableObject"/> representation of <see cref="SegmentContent"/>.
/// </summary>
[CreateAssetMenu(fileName = "SegmentContent", menuName = "PEAKLib/SegmentContent", order = 2)]
public class UnitySegmentContent : ScriptableObject, IContent
{
    /// <inheritdoc cref="SegmentContent.Name"/>
    public string Name => Resolve().Name; // Note: this getter must never throw.
    internal static readonly Dictionary<UnitySegmentContent, SegmentContent> s_UnityToSegment = [];

    /// <inheritdoc/>
    public IRegisteredContent Register(ModDefinition owner) => Resolve().Register(owner);

    /// <inheritdoc/>
    public SegmentContent Resolve()
    {
        if (s_UnityToSegment.TryGetValue(this, out var segment))
        {
            return segment;
        }

        segment = new();
        s_UnityToSegment.Add(this, segment);
        return segment;
    }

    /// <inheritdoc/>
    IContent IContent.Resolve() => Resolve();
}
