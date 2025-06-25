using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PEAKLib.Core;

/// <summary>
/// A content registry which can be used for finding out which mod content
/// belong to which mod.
/// </summary>
public static class ContentRegistry
{
    internal static readonly Dictionary<IModContent, IRegisteredModContent> s_RegisteredContent =
    [];

    /// <summary>
    /// Registers <typeparamref name="T"/> <paramref name="modContent"/>
    /// with <see cref="ContentRegistry"/>.
    /// </summary>
    /// <remarks>
    /// This doesn't register the content with the game.
    /// You should only use this if you are implementing a new <see cref="IModContent"/>
    /// type for PEAKLib.
    /// </remarks>
    /// <typeparam name="T">The mod content type.</typeparam>
    /// <param name="modContent">The mod content.</param>
    /// <param name="owner">The owner of the content.</param>
    /// <returns>The registered <typeparamref name="T"/> representation.</returns>
    public static RegisteredModContent<T> Register<T>(T modContent, ModDefinition owner)
        where T : IModContent<T> => new(modContent, owner);

    /// <summary>
    /// Checks if <paramref name="modContent"/> is registered.
    /// </summary>
    /// <param name="modContent">The <see cref="IModContent"/> to check for registration.</param>
    /// <returns>Whether or not <paramref name="modContent"/> is registered.</returns>
    public static bool IsRegistered(this IModContent modContent) =>
        s_RegisteredContent.ContainsKey(modContent.Resolve());

    /// <summary>
    /// Tries to get the <see cref="RegisteredModContent{T}"/> of
    /// <typeparamref name="T"/> <paramref name="modContent"/> if
    /// it has been registered.
    /// </summary>
    /// <typeparam name="T">The mod content type.</typeparam>
    /// <param name="modContent">The mod content.</param>
    /// <param name="registeredContent">The found registered mod content.</param>
    /// <returns>Whether or not <paramref name="modContent"/> was registered.</returns>
    public static bool TryGetRegisteredMod<T>(
        this T modContent,
        [NotNullWhen(true)] out RegisteredModContent<T>? registeredContent
    )
        where T : IModContent<T>
    {
        registeredContent = default;
        if (!s_RegisteredContent.TryGetValue(modContent, out var registered))
        {
            return false;
        }

        registeredContent = (RegisteredModContent<T>)registered;
        return true;
    }

    /// <summary>
    /// Tries to get the <see cref="RegisteredModContent{T}"/> of
    /// <typeparamref name="T"/> <paramref name="modContent"/> if
    /// it has been registered and is compatible with type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The mod content type.</typeparam>
    /// <param name="modContent">The mod content.</param>
    /// <param name="registeredContent">The found registered mod content.</param>
    /// <returns>Whether or not <paramref name="modContent"/> was registered
    /// and is compatible with type <typeparamref name="T"/>.</returns>
    public static bool TryResolveAndGetRegisteredMod<T>(
        this T modContent,
        [NotNullWhen(true)] out RegisteredModContent<T>? registeredContent
    )
        where T : IModContent
    {
        registeredContent = default;
        if (!s_RegisteredContent.TryGetValue(modContent.Resolve(), out var registered))
        {
            return false;
        }

        if (typeof(T).IsAssignableFrom(registered.Content.GetType()))
        {
            registeredContent = (RegisteredModContent<T>)registered;
            return true;
        }

        return false;
    }
}
