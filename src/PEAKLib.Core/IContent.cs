using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// A generic interface for a registrable mod content.
/// </summary>
public interface IContent<T> : IContent
    where T : IContent<T>
{
    /// <summary>
    /// Registers this <typeparamref name="T"/> with the game.
    /// </summary>
    /// <param name="owner">The <see cref="ModDefinition"/>
    /// who owns this <typeparamref name="T"/>.</param>
    /// <returns>The registered <typeparamref name="T"/> representation.</returns>
    public new RegisteredContent<T> Register(ModDefinition owner);
}

/// <summary>
/// A non-generic interface for a registrable mod content.
/// </summary>
public interface IContent
{
    /// <summary>
    /// Gets the name of this content.
    /// </summary>
    /// <remarks>
    /// This should never throw.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Registers this content with the game.
    /// </summary>
    /// <param name="owner">The <see cref="ModDefinition"/> who owns this content.</param>
    /// <returns>The registered content representation.</returns>
    public IRegisteredContent Register(ModDefinition owner);

    /// <summary>
    /// If this content is a <see cref="ScriptableObject"/>, returns
    /// the real representation of the <see cref="IContent"/>.
    /// </summary>
    /// <returns>Returns the real representation of the <see cref="IContent"/>.</returns>
    public IContent Resolve();
}
