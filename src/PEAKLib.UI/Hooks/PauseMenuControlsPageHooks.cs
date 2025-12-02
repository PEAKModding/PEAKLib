using System.Collections.Generic;
using System.Linq;
using Md.PauseMenuControlsPage;
using MonoDetour;
using MonoDetour.HookGen;
using PEAKLib.Core;
using PEAKLib.UI.Elements;
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

            var reset = control?.transform.Find("ResetButton").gameObject;
            var warning = control?.transform.Find("Warning").gameObject;
            var inputIcon = control?.gameObject.GetComponentInChildren<InputIcon>();

            List<GameObject?> CannotBeNull = [control, reset, warning];

            if (CannotBeNull.Any(o => o == null) || inputIcon == null)
            {
                ThrowHelper.ThrowIfArgumentNull(
                    control,
                    "Unable to get valid control gameobject for control button templates!"
                );
                ThrowHelper.ThrowIfArgumentNull(reset, "Unable to get reset button template!");
                ThrowHelper.ThrowIfArgumentNull(
                    warning,
                    "Unable to get bind warning object for template!"
                );
                ThrowHelper.ThrowIfArgumentNull(
                    inputIcon,
                    "Unable to get valid inputIcon for template!"
                );
                return;
            }

            //Control Page Re-Used Buttons
            Templates.ResetBindButton = Object.Instantiate(reset)!;
            Templates.ResetBindButton.name = "PeakUIResetBindButton";
            Object.DontDestroyOnLoad(Templates.ResetBindButton);

            Templates.BindWarningButton = Object.Instantiate(warning)!;
            Templates.BindWarningButton.name = "PeakUIBindWarningButton";
            Object.DontDestroyOnLoad(Templates.BindWarningButton);

            //Sprite Sheets
            Templates.PS5SpriteSheet = inputIcon!.ps5Sprites;
            Templates.PS4SpriteSheet = inputIcon!.ps4Sprites;
            Templates.XboxSpriteSheet = inputIcon!.xboxSprites;
            Templates.SwitchSpriteSheet = inputIcon!.switchSprites;
            Templates.KeyboardSpriteSheet = inputIcon!.keyboardSprites;
            InitComplete = true;
        }
    }
}
