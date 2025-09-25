using MonoDetour;
using MonoDetour.HookGen;
using On.PauseMenuControlsPage;
using PEAKLib.Core;
using PEAKLib.UI.Elements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zorro.Core;

namespace PEAKLib.UI.Hooks;

[MonoDetourTargets(typeof(PauseMenuControlsPage))]
static class PauseMenuControlsPageHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Awake.Prefix(Prefix_Start);
    }

    internal static bool InitComplete = false;
    static void Prefix_Start(PauseMenuControlsPage self)
    {
        if (!InitComplete)
        {
            var control = self.transform.FindChildRecursive("UI_Control_KB_MoveForward").gameObject;
            UIPlugin.Log.LogDebug($"control is null - {control == null}");

            var rebind = control?.transform.Find("Rebind").gameObject;
            var reset = control?.transform.Find("ResetButton").gameObject;
            var warning = control?.transform.Find("Warning").gameObject;

            List<GameObject> CannotBeNull = [control, rebind, reset, warning];

            if (CannotBeNull.Any(o => o == null!))
            {
                ThrowHelper.ThrowIfArgumentNull(control, "Unable to get valid control gameobject for control button templates!");
                ThrowHelper.ThrowIfArgumentNull(rebind, "Unable to get rebind button template!");
                ThrowHelper.ThrowIfArgumentNull(reset, "Unable to get reset button template!");
                ThrowHelper.ThrowIfArgumentNull(warning, "Unable to get bind warning object for template!");
                return;
            }

            Templates.ResetBindButton = Object.Instantiate(reset)!;
            Templates.ResetBindButton.name = "PeakUIResetBindButton";
            Object.DontDestroyOnLoad(Templates.ResetBindButton);

            Templates.BindWarningButton = Object.Instantiate(warning)!;
            Templates.BindWarningButton.name = "PeakUIBindWarningButton";
            Object.DontDestroyOnLoad(Templates.BindWarningButton);
            InitComplete = true;
        }

        //MenuAPI.controlsMenuBuilderDelegate?.Invoke(self.gameObject.transform);
    }
}