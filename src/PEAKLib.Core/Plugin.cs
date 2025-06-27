using System;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using UnityEngine;

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

        PlayerHandler.OnCharacterRegistered += (Character character) =>
        {
            Networking.networkManager = character.gameObject.AddComponent<NetworkManager>();
        };

        Log.LogInfo($"Plugin {Name} is loaded!");

    }
}
