using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PEAKLib.Core;

internal static class ThrowHelper
{
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ThrowIfArgumentNullOrWriteSpace(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string name = ""
    )
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException($"'{name}' must not be null or whitespace.");
        return argument;
    }

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
