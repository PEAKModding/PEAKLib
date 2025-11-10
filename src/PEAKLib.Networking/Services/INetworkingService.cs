using System;
using PEAKLib.Networking.Modules;

namespace PEAKLib.Networking.Services
{
    /// <summary>
    /// </summary>
    public enum ReliableType { Unreliable, Reliable, UnreliableNoDelay }

    /// <summary>
    /// Backend-agnostic networking service.
    /// </summary>
    public interface INetworkingService
    {
        /// <summary>
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// </summary>
        bool InLobby { get; }
        /// <summary>
        /// </summary>
        ulong HostSteamId64 { get; } // 0 if none
        /// <summary>
        /// </summary>
        string HostIdString { get; }

        /// <summary>
        /// </summary>
        void Initialize();
        /// <summary>
        /// </summary>
        void Shutdown();

        /// <summary>
        /// </summary>
        void CreateLobby(int maxPlayers = 8);
        /// <summary>
        /// </summary>
        void JoinLobby(ulong lobbySteamId64);
        /// <summary>
        /// </summary>
        void LeaveLobby();
        /// <summary>
        /// </summary>
        void InviteToLobby(ulong steamId64);

        /// <summary>
        /// Register all [CustomRPC] methods on this instance; returns a disposable token to deregister automatically.
        /// </summary>
        IDisposable RegisterNetworkObject(object instance, uint modId, int mask = 0);
        /// <summary>
        /// </summary>
        void DeregisterNetworkObject(object instance, uint modId, int mask = 0);

        /// <summary>
        /// Broadcast to all players (reliable/unreliable). Loopback to local is automatic.
        /// </summary>
        void RPC(uint modId, string methodName, ReliableType reliable, params object[] parameters);

        /// <summary>
        /// Send to a single Steam player.
        /// </summary>
        void RPCTarget(uint modId, string methodName, ulong targetSteamId64, ReliableType reliable, params object[] parameters);

        /// <summary>
        /// Send to host only (if in lobby)
        /// </summary>
        void RPCToHost(uint modId, string methodName, ReliableType reliable, params object[] parameters);

        /// <summary>
        /// </summary>
        void RegisterLobbyDataKey(string key);
        /// <summary>
        /// </summary>
        void SetLobbyData(string key, object value);
        /// <summary>
        /// </summary>
        T GetLobbyData<T>(string key);

        /// <summary>
        /// </summary>
        void RegisterPlayerDataKey(string key);
        /// <summary>
        /// </summary>
        void SetPlayerData(string key, object value);
        /// <summary>
        /// </summary>
        T GetPlayerData<T>(ulong steamId64, string key);

        /// <summary>
        /// </summary>
        void PollReceive();

        /// <summary>
        /// </summary>
        event Action LobbyCreated;
        /// <summary>
        /// </summary>
        event Action LobbyEntered;
        /// <summary>
        /// </summary>
        event Action LobbyLeft;
        /// <summary>
        /// </summary>
        event Action<ulong> PlayerEntered; // steam64
        /// <summary>
        /// </summary>
        event Action<ulong> PlayerLeft;
        /// <summary>
        /// </summary>
        event Action<string[]> LobbyDataChanged; // keys
        /// <summary>
        /// </summary>
        event Action<ulong, string[]> PlayerDataChanged; // steam64, keys

        /// <summary>
        /// </summary>
        Func<Message, ulong, bool>? IncomingValidator { get; set; }
    }
}
