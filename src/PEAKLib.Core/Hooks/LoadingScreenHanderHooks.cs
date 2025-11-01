#if !UNITY_EDITOR
using System;
using HarmonyLib;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoDetour.Reflection.Unspeakable;
using Md.LoadingScreenHandler;

namespace PEAKLib.Core.Hooks;

[MonoDetourTargets(typeof(LoadingScreenHandler), GenerateControlFlowVariants = true)]
static class LoadingScreenHandlerHooks
{
    static bool loadBundlesStepInjected;

    static readonly EnumeratorFieldReferenceGetter<string> sceneName = LoadSceneProcess
        .StateMachineTarget()
        .EnumeratorFastFieldReference<string>("sceneName");

    [MonoDetourHookInitialize]
    static void Init()
    {
        LoadSceneProcess.ControlFlowPrefixMoveNext(Prefix_LoadSceneProcess_MoveNext);
    }

    private static ReturnFlow Prefix_LoadSceneProcess_MoveNext(
        SpeakableEnumerator<object, LoadingScreenHandler> self,
        ref bool continueEnumeration
    )
    {
        if (self.State is not 0)
            return ReturnFlow.None;

        if (loadBundlesStepInjected is true)
            return ReturnFlow.None;

        if (sceneName(self.Enumerator) is not "Airport")
            return ReturnFlow.None;

        self.Current = BundleLoader.LoadOperationsDeadlineWait(self.This);
        loadBundlesStepInjected = true;
        continueEnumeration = true;
        return ReturnFlow.HardReturn;
    }
}
#endif
