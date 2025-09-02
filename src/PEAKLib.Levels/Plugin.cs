#if !UNITY_EDITOR
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;

namespace PEAKLib.Levels;

/// <summary>
/// BepInEx plugin of PEAKLib.Levels.
/// Depend on this with <c>[BepInDependency(LevelsPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class LevelsPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(LevelsPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
#endif
