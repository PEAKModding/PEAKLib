using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;

namespace PEAKLib.Items;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
[BepInDependency(ItemsPlugin.Id)]
public partial class TestsPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

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

        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
