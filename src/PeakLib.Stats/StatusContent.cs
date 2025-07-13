using System.Collections.Generic;
using PEAKLib.Core;

namespace PEAKLib.Stats;

/// <summary>
/// A PEAKLib <see cref="StatusContent"/>.
/// </summary>
public class StatusContent(Status status) : IContent<StatusContent>
{
    /// <inheritdoc/>
    public string Name => Status.Name;

    /// <inheritdoc/>
    public Status Status { get; } = ThrowHelper.ThrowIfArgumentNull(status);
    internal static List<RegisteredContent<StatusContent>> s_RegisteredStatuses = [];

    /// <inheritdoc/>
    public RegisteredContent<StatusContent> Register(ModDefinition owner)
    {
        var registered = ContentRegistry.Register(this, owner);
        s_RegisteredStatuses.Add(registered);
        CustomStatusManager.RegisterStatus(status, owner);
        return registered;
    }

    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;
}
