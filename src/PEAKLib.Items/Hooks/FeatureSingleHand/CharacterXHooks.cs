using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.Cil.Analysis;
using MonoDetour.HookGen;
using MonoMod.Cil;

namespace PEAKLib.Items.Hooks.FeatureSingleHand;

// This file contains hooks that implement a feature where items don't require
// two hand positions to be held, and thus can be held one handed.

[MonoDetourTargets(typeof(CharacterAnimations))]
[MonoDetourTargets(typeof(CharacterItems))]
static class CharacterXHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        try
        {
            On.CharacterAnimations.ConfigureIK.ILHook(ILHook_CharacterAnimations_ConfigureIK);
            On.CharacterAnimations.HandleIK.ILHook(ILHook_CharacterAnimations_HandleIK);
            On.CharacterItems.AttachItem.ILHook(ILHook_CharacterItems_AttachItem);
        }
        catch (Exception ex)
        {
            ItemsPlugin.Log.LogError($"Exception in single handed items ILHooks: {ex}");
        }
    }

    // Inserts a method call to set IK weights to 0f when a hand position is missing
    // for an item.
    private static void ILHook_CharacterAnimations_HandleIK(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        Instruction rightIk = null!;
        Instruction leftIk = null!;

        w.MatchRelaxed(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterAnimations>(nameof(CharacterAnimations.character)),
                x => x.MatchLdfld<Character>(nameof(Character.data)),
                x => x.MatchLdfld<CharacterData>(nameof(CharacterData.overrideIKForSeconds)),
                x => x.MatchLdcR4(out _),
                x => x.MatchBgtUn(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterAnimations>(nameof(CharacterAnimations.character)),
                x => x.MatchLdfld<Character>(nameof(Character.refs)),
                x => x.MatchLdfld<Character.CharacterRefs>(nameof(Character.CharacterRefs.ikRig)),
                x => x.MatchLdcR4(out _),
                x => x.MatchCallvirt(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterAnimations>(nameof(CharacterAnimations.character)),
                x => x.MatchLdfld<Character>(nameof(Character.refs)),
                x => x.MatchLdfld<Character.CharacterRefs>(nameof(Character.CharacterRefs.ikRight)),
                x => x.MatchLdcR4(out _) && w.SetInstructionTo(ref rightIk, x),
                x => x.MatchCallvirt(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterAnimations>(nameof(CharacterAnimations.character)),
                x => x.MatchLdfld<Character>(nameof(Character.refs)),
                x => x.MatchLdfld<Character.CharacterRefs>(nameof(Character.CharacterRefs.ikLeft)),
                x => x.MatchLdcR4(out _) && w.SetInstructionTo(ref leftIk, x),
                x => x.MatchCallvirt(out _)
            )
            .ThrowIfFailure();

        w.InsertAfter(
            rightIk,
            w.Create(OpCodes.Pop),
            w.Create(OpCodes.Ldarg_0),
            w.CreateCall(GetIKWeightsRight)
        );

        w.InsertAfter(
            leftIk,
            w.Create(OpCodes.Pop),
            w.Create(OpCodes.Ldarg_0),
            w.CreateCall(GetIKWeightsLeft)
        );
    }

    static float GetIKWeightsRight(CharacterAnimations self) => HasHandRight(self) ? 1f : 0f;

    static float GetIKWeightsLeft(CharacterAnimations self) => HasHandLeft(self) ? 1f : 0f;

    // Inserts conditional branches over instructions which set hand positions based
    // on item data so that an item doesn't need both left or right hands.
    private static void ILHook_CharacterAnimations_ConfigureIK(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        var hasLeft = new VariableDefinition(w.Context.Import(typeof(bool)));
        var hasRight = new VariableDefinition(w.Context.Import(typeof(bool)));
        w.Body.Variables.Add(hasLeft);
        w.Body.Variables.Add(hasRight);

        var infoBody = w.Body.CreateInformationalSnapshotEvaluateAll();

        w.MatchRelaxed(x =>
                x.MatchCallvirt<CharacterItems>(nameof(CharacterItems.GetItemPosLeft))
                && w.SetCurrentTo(x)
            )
            .ThrowIfFailure()
            .InsertBranchOverIfFalse(
                w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                w.Create(OpCodes.Ldarg_0),
                w.CreateCall(HasHandLeft),
                w.Create(OpCodes.Dup),
                w.Create(OpCodes.Stloc, hasLeft)
            );

        w.MatchRelaxed(x =>
                x.MatchCallvirt<CharacterItems>(nameof(CharacterItems.GetItemPosRight))
                && w.SetCurrentTo(x)
            )
            .ThrowIfFailure()
            .InsertBranchOverIfFalse(
                w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                w.Create(OpCodes.Ldarg_0),
                w.CreateCall(HasHandRight),
                w.Create(OpCodes.Dup),
                w.Create(OpCodes.Stloc, hasRight)
            );

        w.MatchRelaxed(x =>
                x.MatchCallvirt<CharacterItems>(nameof(CharacterItems.GetItemRotRight))
                && w.SetCurrentTo(x)
            )
            .ThrowIfFailure()
            .InsertBranchOverIfFalse(
                w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                w.Create(OpCodes.Ldloc, hasRight)
            );

        w.MatchRelaxed(x =>
                x.MatchCallvirt<CharacterItems>(nameof(CharacterItems.GetItemRotLeft))
                && w.SetCurrentTo(x)
            )
            .ThrowIfFailure()
            .InsertBranchOverIfFalse(
                w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                w.Create(OpCodes.Ldloc, hasLeft)
            );
    }

    // Inserts conditional branches over instructions which initialize hand positions
    // and rotations based on item data so that an item doesn't need both left or right hands.
    private static void ILHook_CharacterItems_AttachItem(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        var hasLeft = new VariableDefinition(w.Context.Import(typeof(bool)));
        var hasRight = new VariableDefinition(w.Context.Import(typeof(bool)));
        w.Body.Variables.Add(hasLeft);
        w.Body.Variables.Add(hasRight);

        var infoBody = w.Body.CreateInformationalSnapshotEvaluateAll();

        int i = 0;

        w.MatchMultipleStrict(
                match =>
                {
                    if (i == 0)
                    {
                        match.InsertBranchOverIfFalse(
                            w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                            w.Create(OpCodes.Ldarg_1),
                            w.CreateCall(HasHandItemRight),
                            w.Create(OpCodes.Dup),
                            w.Create(OpCodes.Stloc, hasRight)
                        );
                    }
                    else
                    {
                        match.InsertBranchOverIfFalse(
                            w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                            w.Create(OpCodes.Ldloc, hasRight)
                        );
                    }
                    i++;
                },
                x => x.MatchLdcI4((int)BodypartType.Hand_R) && w.SetCurrentTo(x),
                x => x.MatchCallvirt<Character>(nameof(Character.GetBodypartRig))
            )
            .ThrowIfFailure();

        i = 0;

        w.MatchMultipleStrict(
                match =>
                {
                    if (i == 0)
                    {
                        match.InsertBranchOverIfFalse(
                            w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                            w.Create(OpCodes.Ldarg_1),
                            w.CreateCall(HasHandItemLeft),
                            w.Create(OpCodes.Dup),
                            w.Create(OpCodes.Stloc, hasLeft)
                        );
                    }
                    else
                    {
                        match.InsertBranchOverIfFalse(
                            w.GetStackSizeZeroAreaContinuous(w.Current, infoBody),
                            w.Create(OpCodes.Ldloc, hasLeft)
                        );
                    }
                    i++;
                },
                x => x.MatchLdcI4((int)BodypartType.Hand_L) && w.SetCurrentTo(x),
                x => x.MatchCallvirt<Character>(nameof(Character.GetBodypartRig))
            )
            .ThrowIfFailure();
    }

    static bool HasHandLeft(CharacterAnimations self) =>
        HasHandItemLeft(self.character.data.currentItem);

    static bool HasHandRight(CharacterAnimations self) =>
        HasHandItemRight(self.character.data.currentItem);

    static bool HasHandItemLeft(Item item) => item.transform.Find("Hand_L");

    static bool HasHandItemRight(Item item) => item.transform.Find("Hand_R");

    // ILWeaver extensions, implement in MonoDetour sometime pls

    static (Instruction start, Instruction end) GetStackSizeZeroAreaContinuous(
        this ILWeaver weaver,
        Instruction start,
        IInformationalMethodBody? informationalBody = null
    )
    {
        informationalBody ??= weaver.Body.CreateInformationalSnapshotEvaluateAll();
        var before = weaver.GetStackSizeZeroBeforeContinuous(start, informationalBody);
        var after = weaver.GetStackSizeZeroAfterContinuous(start, informationalBody);
        return (before, after);
    }

    static ILWeaver InsertBranchOverIfFalse(
        this ILWeaver weaver,
        (Instruction start, Instruction end) range,
        params IEnumerable<Instruction> condition
    ) => InsertBranchOverIfFalse(weaver, range.start, range.end, condition);

    static ILWeaver InsertBranchOverIfFalse(
        this ILWeaver weaver,
        Instruction start,
        Instruction end,
        params IEnumerable<Instruction> condition
    )
    {
        var startIndex = weaver.Instructions.IndexOf(start);
        weaver.InsertBeforeStealLabels(startIndex, weaver.Create(OpCodes.Brfalse, end.Next));
        weaver.InsertBeforeStealLabels(startIndex, condition);
        return weaver;
    }
}
