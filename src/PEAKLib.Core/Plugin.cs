using BepInEx;
using BepInEx.Logging;
using MonoDetour;

namespace PEAKLib.Core;

/// <summary>
/// BepInEx plugin of PEAKLib.Core.
/// </summary>
[BepInAutoPlugin]
public partial class CorePlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        MonoDetourManager.InvokeHookInitializers(typeof(CorePlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
