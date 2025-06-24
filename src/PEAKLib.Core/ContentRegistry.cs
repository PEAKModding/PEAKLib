using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PEAKLib.Core;

public static class ContentRegistry
{
    internal static readonly Dictionary<IModContent, IRegisteredModContent> s_RegisteredContent =
    [];

    public static RegisteredModContent<T> Register<T>(T modContent, ModDefinition owner)
        where T : IModContent<T> => new(modContent, owner);

    public static IRegisteredModContent ResolveAndRegister(
        IModContent modContent,
        ModDefinition owner
    ) => new RegisteredModContent<IModContent>(modContent.Resolve(), owner);

    public static bool IsRegistered(this IModContent modContent) =>
        s_RegisteredContent.ContainsKey(modContent.Resolve());

    public static bool TryGetRegisteredMod<T>(
        this T modContent,
        [MaybeNullWhen(false)] out T registeredContent
    )
        where T : IModContent
    {
        registeredContent = default;
        if (!s_RegisteredContent.TryGetValue(modContent.Resolve(), out var content))
        {
            return false;
        }

        if (typeof(T).IsAssignableFrom(content.GetType()))
        {
            registeredContent = (T)content;
            return true;
        }

        return false;
    }
}
