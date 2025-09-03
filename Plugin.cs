using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;

using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using PEAKLib.Levels.Core;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.TextCore.Text;
using System.Linq;
using Zorro.Core;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.InputSystem.Controls.AxisControl;

namespace PEAKLib.Levels;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
internal partial class LevelsPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = BepInEx.Logging.Logger.CreateLogSource(Name);
    internal static LevelsPlugin Instance { get; private set; } = null!;
    internal static Harmony Harmony = new Harmony("PEAKLevelLoader");

    internal static bool AllowEmbeddedBundles = false;

    private void Awake()
    {
        Instance = this;

        MonoDetourManager.InvokeHookInitializers(typeof(LevelsPlugin).Assembly);
        PatchHooks.ApplyPatches(Harmony);

        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
