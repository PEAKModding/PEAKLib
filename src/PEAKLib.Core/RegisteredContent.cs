using System;

namespace PEAKLib.Core;

/// <summary>
/// Wrapper for a <typeparamref name="T"/> <see cref="IContent"/>
/// with a <see cref="ModDefinition"/> attached to it.
/// </summary>
/// <typeparam name="T">A <see cref="IContent"/> type.</typeparam>
public class RegisteredContent<T> : IRegisteredContent
    where T : IContent
{
    /// <summary>
    /// The <typeparamref name="T"/> content registered.
    /// </summary>
    public T Content { get; }

    IContent IRegisteredContent.Content => Content;

    /// <inheritdoc/>
    public ModDefinition Mod { get; }

    internal RegisteredContent(T content, ModDefinition mod)
    {
        Content = ThrowHelper.ThrowIfArgumentNull(content);
        Mod = ThrowHelper.ThrowIfArgumentNull(mod);

        if (!ContentRegistry.s_RegisteredContent.TryAdd(content, this))
        {
            throw new Exception($"This Content has been registered already: '{content}'");
        }

        mod.RegisteredContent.Add(this);
    }
}

/// <summary>
/// A non-generic wrapper interface for a <see cref="IContent"/>
/// with a <see cref="ModDefinition"/> attached to it.
/// </summary>
public interface IRegisteredContent
{
    /// <summary>
    /// The <see cref="IContent"/> content registered.
    /// </summary>
    public IContent Content { get; }

    /// <summary>
    /// The <see cref="ModDefinition"/> who owns this content.
    /// </summary>
    public ModDefinition Mod { get; }
}
