using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
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
    public static ModDefinition Definition { get; set; } = null!;

    private void Awake()
    {
        // this.LoadBundleWithName(
        //     "peaklibtest.peakbundl",
        //     (ass, mod) =>
        //     {
        //         Log.LogInfo("loaded:" + mod.Id);
        //         mod.RegisterContent();
        //     }
        // );

        // string AssetBundlePath = Path.Combine(
        //     Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
        //     "peaklibtest.peakbundle"
        // );
        // var bundle = AssetBundle.LoadFromFile(AssetBundlePath);

        // var modDefinition = ModDefinition.GetOrCreate(Info);
        // var scriptableObjects = bundle.LoadAllAssets<ScriptableObject>();
        // var contents = scriptableObjects.OfType<IContent>();

        // foreach (var content in contents)
        // {
        //     Log.LogInfo("Adding Content: " + content);
        //     modDefinition.Content.Add(content);
        // }

        // modDefinition.RegisterContent();

        Instance = this;
        Definition = ModDefinition.GetOrCreate(Info.Metadata);

        string AssetBundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "peaklibtest.peakbundle.testball");
        var ballBundle = AssetBundle.LoadFromFile(AssetBundlePath);

        GameObject testBallPrefab = ballBundle.LoadAsset<GameObject>("TestBall.prefab");
        // attach behavior
        testBallPrefab.AddComponent<TestBall>();
        var action = testBallPrefab.AddComponent<Action_TestBallRecolor>();
        action.OnCastFinished = true;
        new ItemContent(testBallPrefab.GetComponent<Item>()).Register(Definition);

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
