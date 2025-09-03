using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal static class TransformExtensions
    {
        public static Transform? FindDeep(this Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName)) return null;
            var found = root.Find(childName);
            if (found != null) return found;

            var tQueue = new Queue<Transform>();
            tQueue.Enqueue(root);
            while (tQueue.Count > 0)
            {
                var t = tQueue.Dequeue();
                if (string.Equals(t.name, childName, StringComparison.OrdinalIgnoreCase))
                    return t;
                for (int i = 0; i < t.childCount; ++i) tQueue.Enqueue(t.GetChild(i));
            }
            return null;
        }

        public static IEnumerable<Transform> GetAllChildrenRecursive(this Transform root)
        {
            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                for (int i = 0; i < t.childCount; ++i)
                {
                    var c = t.GetChild(i);
                    yield return c;
                    stack.Push(c);
                }
            }
        }
    }

}
