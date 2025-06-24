using System;

namespace PEAKLib.Core;

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
    }
}

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
