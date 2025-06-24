using System.Collections.Generic;

namespace PEAKLib.Core;

public static class ContentRegistry
{
    internal static readonly HashSet<IModContent> s_RegisteredContent = [];

    public static RegisteredModContent<T> Register<T>(T modContent, ModDefinition owner)
        where T : IModContent<T> => new(modContent, owner);

    public static bool IsRegistered(this IModContent modContent) =>
        s_RegisteredContent.Contains(modContent);
}
