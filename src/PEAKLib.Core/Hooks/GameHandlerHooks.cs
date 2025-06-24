using MonoDetour;
using MonoDetour.HookGen;
using On.GameHandler;

namespace PEAKLib.Core.Hooks;

[MonoDetourTargets(typeof(GameHandler))]
static class GameHandlerHooks
{
    static bool patchedAwake;

    [MonoDetourHookInitialize]
    static void Init()
    {
        Awake.Postfix(Postfix_Awake);
    }

    static void Postfix_Awake(GameHandler self)
    {
        if (patchedAwake)
        {
            return;
        }

        patchedAwake = true;
        NetworkPrefabManager.Initialize();
    }
}
