using System.Collections.Generic;
using PEAKLib.Core;

namespace PEAKLib.Levels;

// TODO: Implement this.

/// <summary>
/// A registerable PEAKLib level segment.
/// </summary>
public class SegmentContent : IContent<SegmentContent>
{
    /// <summary>
    /// The name of this segment.
    /// </summary>
    public string Name => throw new System.NotImplementedException();
    internal static List<RegisteredContent<SegmentContent>> s_RegisteredSegments = [];

    /// <inheritdoc/>
    public RegisteredContent<SegmentContent> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);
        s_RegisteredSegments.Add(registered);
        return registered;
    }

    /// <inheritdoc/>
    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;
}
