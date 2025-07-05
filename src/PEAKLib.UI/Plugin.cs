using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;

namespace PEAKLib.UI;

/// <summary>
/// BepInEx plugin of PEAKLib.UI.
/// Depend on this with <c>[BepInDependency(UIPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class UIPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(UIPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
