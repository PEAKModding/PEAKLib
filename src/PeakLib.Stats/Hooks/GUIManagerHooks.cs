#if !UNITY_EDITOR
using MonoDetour.DetourTypes;
using MonoDetour;
using MonoDetour.HookGen;
using Md.GUIManager;

namespace PEAKLib.Stats.Hooks;

[MonoDetourTargets(
    typeof(GUIManager),
    GenerateControlFlowVariants = true,
    Members = ["AddStatusFX"]
)]
static class GUIManagerHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        AddStatusFX.ControlFlowPrefix(Prefix_AddStatusFX);
    }

    static ReturnFlow Prefix_AddStatusFX(
        GUIManager self,
        ref CharacterAfflictions.STATUSTYPE type,
        ref float amount
    )
    {
        if (CustomStatusManager.StatusByType(type) != null)
        {
            return ReturnFlow.SkipOriginal;
        }
        return ReturnFlow.None;
    }
}
#endif
