using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// BepInEx plugin of PEAKLib.Items.
/// Depend on this with <c>[BepInDependency(ItemsPlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class ItemsPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(ItemsPlugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
