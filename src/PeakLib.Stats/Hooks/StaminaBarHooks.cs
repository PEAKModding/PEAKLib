#if !UNITY_EDITOR
using MonoDetour;
using MonoDetour.HookGen;
using PEAKLib.Items;
using On.StaminaBar;
using static CharacterAfflictions;
using System;
using UnityEngine;
using UnityEngine.UI;

[MonoDetourTargets(typeof(StaminaBar))]
static class StaminaBarHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Start.Prefix(Prefix_Start);
    }

    static void Prefix_Start(StaminaBar self)
    {
        var afflictions = self.GetComponentsInChildren<BarAffliction>(includeInactive: true);
        GameObject toClone = afflictions[afflictions.Length - 1].gameObject;

        int totalStatuses = Enum.GetValues(typeof(STATUSTYPE)).Length + CustomStatusManager.Length;
        if (afflictions.Length < totalStatuses)
        {
            // add new status effects
            for (int i = afflictions.Length; i < totalStatuses; i++)
            {
                Status status = CustomStatusManager.StatusByType((STATUSTYPE)i);
                // clone previous entry
                BarAffliction newBar = UnityEngine.Object.Instantiate(toClone, toClone.transform.parent).GetComponent<BarAffliction>();
                newBar.rtf = newBar.GetComponent<RectTransform>();
                newBar.afflictionType = status?.Type ?? STATUSTYPE.Injury;
                newBar.size = 0f;
                newBar.icon = newBar.transform.Find("Icon").GetComponent<Image>();
                newBar.icon.sprite = status?.Icon;
                newBar.icon.color = status?.Color ?? Color.white;
                newBar.transform.Find("Fill").GetComponent<Image>().color = status?.Color ?? Color.white;
                newBar.transform.Find("Outline").GetComponent<Image>().color = status?.Color ?? Color.white;
                newBar.gameObject.name = status?.Name;
            }
        }
    }
}
#endif
