#if !UNITY_EDITOR
using System;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using Md.DataEntryValue;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(DataEntryValue), GenerateControlFlowVariants = true)]
static class DataEntryValueHooks
{
    public const byte PEAKLIB_MOD_TYPE_INDEX = 42;

    [MonoDetourHookInitialize]
    static void Init()
    {
        GetTypeValue.Postfix(Postfix_GetTypeValue);
        GetNewFromValue.ControlFlowPrefix(Prefix_GetNewFromValue);
    }

    static void Postfix_GetTypeValue(ref Type type, ref byte returnValue)
    {
        if (returnValue == 0 && type == typeof(ModItemData))
        {
            returnValue = PEAKLIB_MOD_TYPE_INDEX;
        }
    }

    static ReturnFlow Prefix_GetNewFromValue(ref byte value, ref DataEntryValue returnValue)
    {
        if (value == PEAKLIB_MOD_TYPE_INDEX)
        {
            returnValue = new ModItemData();
            return ReturnFlow.SkipOriginal;
        }
        return ReturnFlow.None;
    }
}
#endif
