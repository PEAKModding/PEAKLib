#if !UNITY_EDITOR
using System;
using MonoDetour.HookGen;
using MonoDetour;
using Md.PlayerHandler;

namespace PEAKLib.Core.Hooks;

[MonoDetourTargets(typeof(PlayerHandler))]
static class CharacterRegistrationHooks
{
    public static event Action<Character>? OnCharacterAdded;

    [MonoDetourHookInitialize]
    static void Init()
    {
        RegisterCharacter.Postfix(Postfix_RegisterCharacter);
    }

    private static void Postfix_RegisterCharacter(ref Character character)
    {
        OnCharacterAdded?.Invoke(character);
    }
}
#endif
