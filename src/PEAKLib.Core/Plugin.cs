using System;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// BepInEx plugin of PEAKLib.Core.
/// Depend on this with <c>[BepInDependency(CorePlugin.Id)]</c>.
/// </summary>
[BepInAutoPlugin]
public partial class CorePlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        MonoDetourManager.InvokeHookInitializers(typeof(CorePlugin).Assembly);

        PlayerHandler.OnCharacterRegistered += (Character character) =>
        {
            Networking.s_NetworkManager = character.gameObject.AddComponent<NetworkManager>();
        };

        Log.LogInfo($"Plugin {Name} is loaded!");

    }
}
