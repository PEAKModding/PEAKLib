using System;
using System.Linq;

namespace PEAKLib.Core.Extensions;

internal static class ActionExtensions
{
    public static void SafeInvoke(this Action @delegate)
    {
        foreach (var callback in @delegate.GetInvocationList().Cast<Action>())
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                CorePlugin.Log.LogError($"Unhandled exception in callback: {ex}");
            }
        }
    }

    public static void SafeInvoke<T1>(this Action<T1> @delegate, T1 t1)
    {
        foreach (var callback in @delegate.GetInvocationList().Cast<Action<T1>>())
        {
            try
            {
                callback(t1);
            }
            catch (Exception ex)
            {
                CorePlugin.Log.LogError($"Unhandled exception in callback: {ex}");
            }
        }
    }
}
