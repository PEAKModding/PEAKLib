#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using MonoDetour;
using MonoDetour.HookGen;
using On.DataEntryValue;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(DataEntryValue))]
static class DataEntryValueHooks
{
    public const byte PEAKLIB_MOD_TYPE_INDEX = 42;

    [MonoDetourHookInitialize]
    static void Init()
    {
        GetTypeValue.Postfix(Postfix_GetTypeValue);
        GetNewFromValue.Prefix(Prefix_GetNewFromValue);
        GetNewFromValue.Postfix(Postfix_GetNewFromValue);
    }

    static void Postfix_GetTypeValue(ref Type type, ref byte returnValue)
    {
        if (returnValue == 0 && type == typeof(ModItemData))
        {
            returnValue = PEAKLIB_MOD_TYPE_INDEX;
        }
    }

    // unlike Harmony, there doesn't appear to be a way to suppress the call after a prefix with MonoDetour
    // so we need this horrid hack tracking (threadId -> arg)
    // otherwise GetNewFromValue will throw
    static Dictionary<int, byte> Args_GetNewFromValue = new Dictionary<int, byte>();
    static void Prefix_GetNewFromValue(ref byte value)
    {
        if (value == PEAKLIB_MOD_TYPE_INDEX)
        {
            Args_GetNewFromValue[Environment.CurrentManagedThreadId] = value;
            value = 1;
        }
    }
    static void Postfix_GetNewFromValue(ref byte value, ref DataEntryValue returnValue)
    {
        int thread = Environment.CurrentManagedThreadId;
        if (Args_GetNewFromValue.ContainsKey(thread))
        {
            value = Args_GetNewFromValue[thread];
            Args_GetNewFromValue.Remove(thread);
            returnValue = new ModItemData();
        }
    }
}
#endif
