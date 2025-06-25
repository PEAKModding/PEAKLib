using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// A generic interface for a registrable mod content.
/// </summary>
public interface IModContent<T> : IModContent
    where T : IModContent<T>
{
    /// <summary>
    /// Registers this <typeparamref name="T"/> with the game.
    /// </summary>
    /// <param name="owner">The <see cref="ModDefinition"/>
    /// who owns this <typeparamref name="T"/>.</param>
    /// <returns>The registered <typeparamref name="T"/> representation.</returns>
    public new RegisteredModContent<T> Register(ModDefinition owner);
}

/// <summary>
/// A non-generic interface for a registrable mod content.
/// </summary>
public interface IModContent
{
    /// <summary>
    /// Registers this content with the game.
    /// </summary>
    /// <param name="owner">The <see cref="ModDefinition"/> who owns this content.</param>
    /// <returns>The registered content representation.</returns>
    public IRegisteredModContent Register(ModDefinition owner);

    /// <summary>
    /// If this content is a <see cref="ScriptableObject"/>, returns
    /// the real representation of the <see cref="IModContent"/>.
    /// </summary>
    /// <returns>Returns the real representation of the <see cref="IModContent"/>.</returns>
    public IModContent Resolve();
}
