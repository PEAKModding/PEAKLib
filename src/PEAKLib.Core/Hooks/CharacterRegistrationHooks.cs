#if !UNITY_EDITOR
using System;
using MonoDetour.HookGen;
using MonoDetour;
using UnityEngine;
using MonoMod.RuntimeDetour.HookGen;

namespace PEAKLib.Core.Hooks;

[MonoDetourTargets(typeof(PlayerHandler))]
static class CharacterRegistrationHooks
{
    public static event Action<Character> OnCharacterAdded;

    private delegate void RegisterCharacterDelegate(Action<Character> orig, Character character);

    [MonoDetourHookInitialize]
    static void Init()
    {
        HookEndpointManager.Add(
            typeof(PlayerHandler).GetMethod("RegisterCharacter",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
            (RegisterCharacterDelegate)RegisterCharacterHook
        );
    }

    private static void RegisterCharacterHook(Action<Character> orig, Character character)
    {
        orig(character);
        OnCharacterAdded?.Invoke(character);
    }
}
#endif