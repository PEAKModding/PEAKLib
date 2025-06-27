using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PEAKLib.Core;

/// <summary>
/// A collection of throw helper methods.
/// </summary>
public static class ThrowHelper
{
    /// <summary>
    /// Throw if the specified <paramref name="field"/> is null or whitespace.
    /// </summary>
    /// <param name="field">The <paramref name="field"/> to check.</param>
    /// <param name="name">The name of <paramref name="field"/>.</param>
    /// <returns>A non-null string <paramref name="field"/>.</returns>
    /// <exception cref="NullReferenceException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ThrowIfFieldNullOrWhiteSpace(
        [NotNull] string? field,
        [CallerArgumentExpression(nameof(field))] string name = ""
    )
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new NullReferenceException(
                $"Field or property '{name}' must not be null or whitespace."
            );
        return field;
    }

    /// <summary>
    /// Throw if <paramref name="field"/> is null.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="field"/>.</typeparam>
    /// <param name="field">The <paramref name="field"/> to check.</param>
    /// <param name="name">The name of <paramref name="field"/>.</param>
    /// <returns>A non-null <typeparamref name="T"/> <paramref name="field"/>.</returns>
    /// <exception cref="NullReferenceException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfFieldNull<T>(
        [NotNull] T? field,
        [CallerArgumentExpression(nameof(field))] string name = ""
    )
    {
        if (field == null)
            throw new NullReferenceException($"Field or property '{name}' must not be null.");
        return field;
    }

    /// <summary>
    /// Throw if the specified <paramref name="argument"/> is null or whitespace.
    /// </summary>
    /// <param name="argument">The <paramref name="argument"/> to check.</param>
    /// <param name="name">The name of <paramref name="argument"/>.</param>
    /// <returns>A non-null string <paramref name="argument"/>.</returns>
    /// <exception cref="NullReferenceException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ThrowIfArgumentNullOrWhiteSpace(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string name = ""
    )
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException($"'{name}' must not be null or whitespace.");
        return argument;
    }

    /// <summary>
    /// Throw if <paramref name="argument"/> is null.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="argument"/>.</typeparam>
    /// <param name="argument">The <paramref name="argument"/> to check.</param>
    /// <param name="name">The name of <paramref name="argument"/>.</param>
    /// <returns>A non-null <typeparamref name="T"/> <paramref name="argument"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfArgumentNull<T>(
        [NotNull] T? argument,
        [CallerArgumentExpression(nameof(argument))] string name = ""
    )
    {
        if (argument is null)
            ThrowArgumentNull(name);
        return argument;
    }

    [DoesNotReturn]
    private static void ThrowArgumentNull(string argName)
    {
        throw new ArgumentNullException(argName);
    }
}
