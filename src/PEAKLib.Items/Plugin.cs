using BepInEx;
using BepInEx.Logging;
using MonoDetour;

namespace PEAKLib.Items;

/// <summary>
/// BepInEx plugin of PEAKLib.Items.
/// </summary>
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        MonoDetourManager.InvokeHookInitializers(typeof(Plugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
