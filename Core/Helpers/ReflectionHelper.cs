using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal static class ReflectionHelpers
    {
        public static Type? FindType(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(shortName, false, true);
                    if (t != null) return t;

                    foreach (var type in asm.GetTypes())
                    {
                        if (type.Name.Equals(shortName, StringComparison.OrdinalIgnoreCase))
                            return type;
                    }
                }
                catch { }
            }
            return null;
        }

        public static object? GetPrivateField(object target, string fieldName)
        {
            if (target == null) return null;
            var t = target.GetType();
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f?.GetValue(target);
        }

        public static bool SetMemberValue(object target, string memberName, object? value)
        {
            if (target == null) return false;
            var t = target.GetType();
            var f = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                f.SetValue(target, value);
                return true;
            }
            var p = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite)
            {
                p.SetValue(target, value);
                return true;
            }
            return false;
        }

        public static bool TryGetStaticFieldOrProperty(Type type, string name, out object? value)
        {
            value = null;
            if (type == null) return false;
            var f = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) { value = f.GetValue(null); return true; }
            var p = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null) { value = p.GetValue(null); return true; }
            return false;
        }

        public static IEnumerable<Type> FindTypesAssignableTo(Type baseType)
        {
            var result = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t == null) continue;
                        if (baseType.IsAssignableFrom(t)) result.Add(t);
                    }
                }
                catch { }
            }
            return result;
        }

        public static object? InvokeMethodSafe(object target, string methodName, params object[] args)
        {
            if (target == null) return null;
            var t = target.GetType();
            var m = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null) return null;
            return m.Invoke(target, args);
        }
    }
}
