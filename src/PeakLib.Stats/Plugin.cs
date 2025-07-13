#if !UNITY_EDITOR
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;

namespace PEAKLib.Stats;

/// <summary>
/// BepInEx plugin of PEAKLib.Stats.
/// Depend on this with <c>[BepInDependency(StatsPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class StatsPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(StatsPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
#endif
