using System;

namespace PEAKLib.Core;

/// <summary>
/// Wrapper for a <typeparamref name="T"/> <see cref="IModContent"/>
/// with a <see cref="ModDefinition"/> attached to it.
/// </summary>
/// <typeparam name="T">A <see cref="IModContent"/> type.</typeparam>
public class RegisteredModContent<T> : IRegisteredModContent
    where T : IModContent
{
    /// <summary>
    /// The <typeparamref name="T"/> content registered.
    /// </summary>
    public T Content { get; }

    IModContent IRegisteredModContent.Content => Content;

    /// <inheritdoc/>
    public ModDefinition Mod { get; }

    internal RegisteredModContent(T content, ModDefinition mod)
    {
        Content = ThrowHelper.ThrowIfArgumentNull(content);
        Mod = ThrowHelper.ThrowIfArgumentNull(mod);

        if (!ContentRegistry.s_RegisteredContent.TryAdd(content, this))
        {
            throw new Exception($"This Content has been registered already: '{content}'");
        }

        mod.Content.Add(content);
    }
}

/// <summary>
/// A non-generic wrapper interface for a <see cref="IModContent"/>
/// with a <see cref="ModDefinition"/> attached to it.
/// </summary>
public interface IRegisteredModContent
{
    /// <summary>
    /// The <see cref="IModContent"/> content registered.
    /// </summary>
    public IModContent Content { get; }

    /// <summary>
    /// The <see cref="ModDefinition"/> who owns this content.
    /// </summary>
    public ModDefinition Mod { get; }
}
