using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Tests;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PEAKLib.Items;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
[BepInDependency(ItemsPlugin.Id)]
public partial class TestsPlugin : BaseUnityPlugin
{
    public static TestsPlugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    private void Awake()
    {
        Instance = this;

        this.LoadBundleWithName(
            "peaklibtest.peakbundle.testball",
            bundle =>
            {
                var testBallPrefab = bundle.LoadAsset<GameObject>("TestBall.prefab");
                // attach behavior
                testBallPrefab.AddComponent<TestBall>();
                var action = testBallPrefab.AddComponent<Action_TestBallRecolor>();
                action.OnCastFinished = true;
                bundle.Mod.RegisterContent();
            }
        );

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
