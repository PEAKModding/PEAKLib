using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PEAKLib.Core;

/// <summary>
/// A content registry which can be used for finding out which mod content
/// belong to which mod.
/// </summary>
public static class ContentRegistry
{
    internal static readonly Dictionary<IContent, IRegisteredContent> s_RegisteredContent = [];

    /// <summary>
    /// Registers <typeparamref name="T"/> <paramref name="modContent"/>
    /// with <see cref="ContentRegistry"/>.
    /// </summary>
    /// <remarks>
    /// This doesn't register the content with the game.
    /// You should only use this if you are implementing a new <see cref="IContent"/>
    /// type for PEAKLib.
    /// </remarks>
    /// <typeparam name="T">The mod content type.</typeparam>
    /// <param name="modContent">The mod content.</param>
    /// <param name="owner">The owner of the content.</param>
    /// <returns>The registered <typeparamref name="T"/> representation.</returns>
    public static RegisteredContent<T> Register<T>(T modContent, ModDefinition owner)
        where T : IContent<T> => new(modContent, owner);

    /// <summary>
    /// Checks if <paramref name="modContent"/> is registered.
    /// </summary>
    /// <param name="modContent">The <see cref="IContent"/> to check for registration.</param>
    /// <returns>Whether or not <paramref name="modContent"/> is registered.</returns>
    public static bool IsRegistered(this IContent modContent) =>
        s_RegisteredContent.ContainsKey(modContent.Resolve());

    /// <summary>
    /// Tries to get the <see cref="RegisteredContent{T}"/> of
    /// <typeparamref name="T"/> <paramref name="modContent"/>
    /// if it has been registered.
    /// </summary>
    /// <inheritdoc cref="TryResolveAndGetRegisteredMod{T}(T, out IRegisteredContent?)"/>
    public static bool TryGetRegisteredMod<T>(
        this T modContent,
        [NotNullWhen(true)] out RegisteredContent<T>? registeredContent
    )
        where T : IContent<T>
    {
        registeredContent = default;
        if (!s_RegisteredContent.TryGetValue(modContent, out var registered))
        {
            return false;
        }

        registeredContent = (RegisteredContent<T>)registered;
        return true;
    }

    /// <summary>
    /// Tries to get the <see cref="IRegisteredContent"/> of
    /// <typeparamref name="T"/> <paramref name="modContent"/>
    /// if it has been registered.
    /// </summary>
    /// <typeparam name="T">The mod content type.</typeparam>
    /// <param name="modContent">The mod content.</param>
    /// <param name="registeredContent">The found registered mod content.</param>
    /// <returns>Whether or not <paramref name="modContent"/> was registered.</returns>
    public static bool TryResolveAndGetRegisteredMod<T>(
        this T modContent,
        [NotNullWhen(true)] out IRegisteredContent? registeredContent
    )
        where T : IContent
    {
        registeredContent = default;
        if (!s_RegisteredContent.TryGetValue(modContent.Resolve(), out var registered))
        {
            return false;
        }

        registeredContent = registered;
        return true;
    }
}
