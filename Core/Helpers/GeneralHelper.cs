using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using PEAKLib.Core;
using PEAKLib.Levels.Core;

internal static class GeneralHelper
{
    public static string? ArrayFindIgnoreCase(string[] arr, string contains, string? mustEndWith = null)
    {
        if (arr == null || contains == null) return null;
        foreach (var a in arr)
        {
            var low = a.ToLowerInvariant();
            if (!string.IsNullOrEmpty(contains) && low.Contains(contains.ToLowerInvariant()))
            {
                if (string.IsNullOrEmpty(mustEndWith) || a.EndsWith(mustEndWith, StringComparison.OrdinalIgnoreCase))
                    return a;
            }
        }
        return null;
    }

    public static Transform? FindDeepByName(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase)) return root;
        var stack = new System.Collections.Generic.Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase)) return t;
            for (int i = 0; i < t.childCount; ++i) stack.Push(t.GetChild(i));
        }
        return null;
    }
}
