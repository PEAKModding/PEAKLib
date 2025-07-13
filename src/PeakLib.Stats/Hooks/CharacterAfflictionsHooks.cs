#if !UNITY_EDITOR
using System;
using MonoDetour;
using MonoDetour.HookGen;
using On.CharacterAfflictions;
using UnityEngine;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(CharacterAfflictions))]
static class CharacterAfflictionsHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        InitStatusArrays.Postfix(Postfix_InitStatusArrays);
        UpdateNormalStatuses.Postfix(Postfix_UpdateNormalStatuses);
        StatusSFX.Postfix(Postfix_StatusSFX);
        PlayParticle.Postfix(Postfix_PlayParticle);
        GetStatusCap.Postfix(Postfix_GetStatusCap);
        ClearAllStatus.Postfix(Postfix_ClearAllStatus);
    }

    // make space in the status array for custom status effects
    static void Postfix_InitStatusArrays(CharacterAfflictions self)
    {
        self.currentStatuses = new float[Enum.GetNames(typeof(CharacterAfflictions.STATUSTYPE)).Length + CustomStatusManager.Length];
        self.currentIncrementalStatuses = new float[self.currentStatuses.Length];
        self.currentDecrementalStatuses = new float[self.currentStatuses.Length];
        self.lastAddedStatus = new float[self.currentStatuses.Length];
        self.lastAddedIncrementalStatus = new float[self.currentStatuses.Length];
    }

    static void Postfix_UpdateNormalStatuses(CharacterAfflictions self)
    {
        if (self.character.IsLocal)
        {
            foreach (var status in CustomStatusManager.Statuses)
            {
                if (status.Update != null)
                {
                    status.Update(self, status);
                }
                else
                {
                    if (self.GetCurrentStatus(status.Type) > 0f && Time.time - self.LastAddedStatus(status.Type) > status.ReductionCooldown)
                    {
                        self.SubtractStatus(status.Type, status.ReductionPerSecond * Time.deltaTime);
                    }
                }
            }
        }
    }

    static void Postfix_StatusSFX(CharacterAfflictions self, ref CharacterAfflictions.STATUSTYPE status, ref float amount)
    {
        Status statusObj = CustomStatusManager.StatusByType(status);
        if (statusObj?.SFX != null)
        {
            statusObj.SFX.Play();
        }
    }

    static void Postfix_PlayParticle(CharacterAfflictions self, ref CharacterAfflictions.STATUSTYPE status)
    {
        Status statusObj = CustomStatusManager.StatusByType(status);
        if (statusObj != null)
        {
            self.character.refs.customization.PulseStatus(statusObj.Color);
        }
    }

    static void Postfix_GetStatusCap(CharacterAfflictions self, ref CharacterAfflictions.STATUSTYPE status, ref float returnValue)
    {
        returnValue = CustomStatusManager.StatusByType(status)?.MaxAmount ?? returnValue;
    }

    static void Postfix_ClearAllStatus(CharacterAfflictions self, ref bool excludeCurse)
    {
        foreach (Status status in CustomStatusManager.Statuses)
        {
            // printouts left in for consistency with vanilla function
            Debug.Log("Clearing status: " + status.Type);
            if (status.AllowClear)
            {
                Debug.Log($"Current: {status.Type}, amount {self.character.refs.afflictions.GetCurrentStatus(status.Type)}");
                Debug.Log($"SetStatus status: {status.Type}");
                self.character.refs.afflictions.SetStatus(status.Type, 0f);
            }
        }
    }
}
#endif
