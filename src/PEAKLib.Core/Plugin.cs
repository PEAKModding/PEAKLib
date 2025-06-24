using BepInEx;
using BepInEx.Logging;

namespace PEAKLib.Core;

/// <summary>
/// BepInEx plugin of PEAKLib.Core.
/// </summary>
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
