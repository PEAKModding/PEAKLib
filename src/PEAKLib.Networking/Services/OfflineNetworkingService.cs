#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using PEAKLib.Networking.Modules;
using PEAKLib.Networking.Services;
using UnityEngine;

namespace PEAKLib.Networking.Services
{
    /// <summary>
    /// Offline in-process implementation of INetworkingService.
    /// </summary>
    public class OfflineNetworkingService : INetworkingService
    {
        /// <summary>
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// </summary>
        public bool InLobby { get; private set; }
        /// <summary>
        /// </summary>
        public ulong HostSteamId64 { get; private set; } = 1UL;
        /// <summary>
        /// </summary>
        public string HostIdString => HostSteamId64.ToString();

        /// <summary>
        /// </summary>
        public event Action LobbyCreated;
        /// <summary>
        /// </summary>
        public event Action LobbyEntered;
        /// <summary>
        /// </summary>
        public event Action LobbyLeft;
        /// <summary>
        /// </summary>
        public event Action<ulong> PlayerEntered;
        /// <summary>
        /// </summary>
        public event Action<ulong> PlayerLeft;
        /// <summary>
        /// </summary>
        public event Action<string[]> LobbyDataChanged;
        /// <summary>
        /// </summary>
        public event Action<ulong, string[]> PlayerDataChanged;

        /// <summary>
        /// </summary>
        public Func<Message, ulong, bool>? IncomingValidator { get; set; }

        readonly Dictionary<uint, Dictionary<string, List<MessageHandler>>> rpcs = new();
        readonly Dictionary<string, string> lobbyData = new();
        readonly Dictionary<ulong, Dictionary<string, string>> perPlayerData = new();
        readonly HashSet<string> lobbyKeys = new();
        readonly HashSet<string> playerKeys = new();

        readonly SlidingWindowRateLimiter rateLimiter = new(100, TimeSpan.FromSeconds(1));

        readonly Dictionary<ulong, byte[]> perPeerSymmetricKey = new();
        byte[]? globalSharedSecret;
        HMACSHA256? globalHmac;

        long _nextMessageId = 0;
        private ulong NextMessageId() => (ulong)System.Threading.Interlocked.Increment(ref _nextMessageId);

        /// <summary>
        /// </summary>
        public ulong LocalSteamId { get; private set; } = 1000;

        class Token : IDisposable { Action on; public Token(Action on) { this.on = on; } public void Dispose() => on(); }

        /// <summary>
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            var rng = RandomNumberGenerator.Create();
            var k = new byte[32];
            rng.GetBytes(k);
            perPeerSymmetricKey[LocalSteamId] = k;
            IsInitialized = true;
        }

        /// <summary>
        /// </summary>
        public void Shutdown()
        {
            IsInitialized = false;
            rpcs.Clear();
            perPeerSymmetricKey.Clear();
            globalHmac?.Dispose(); globalHmac = null;
        }

        /// <summary>
        /// </summary>
        public void CreateLobby(int maxPlayers = 8)
        {
            InLobby = true;
            HostSteamId64 = LocalSteamId;
            perPlayerData[LocalSteamId] = new Dictionary<string, string>();
            LobbyCreated?.Invoke();
            LobbyEntered?.Invoke();
        }

        /// <summary>
        /// </summary>
        public void JoinLobby(ulong lobbySteamId64)
        {
            InLobby = true;
            HostSteamId64 = lobbySteamId64;
            if (!perPlayerData.ContainsKey(LocalSteamId)) perPlayerData[LocalSteamId] = new Dictionary<string, string>();
            LobbyEntered?.Invoke();
            PlayerEntered?.Invoke(LocalSteamId);
        }

        /// <summary>
        /// </summary>
        public void LeaveLobby()
        {
            InLobby = false;
            LobbyLeft?.Invoke();
        }

        /// <summary>
        /// </summary>
        public void InviteToLobby(ulong steamId64)
        {
            PlayerEntered?.Invoke(steamId64);
        }

        /// <summary>
        /// </summary>
        public IDisposable RegisterNetworkObject(object instance, uint modId, int mask = 0)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var t = instance.GetType();
            int registered = 0;
            foreach (var method in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attrs = method.GetCustomAttributes(false).OfType<CustomRPCAttribute>().ToArray();
                if (attrs.Length == 0) continue;
                if (!rpcs.ContainsKey(modId)) rpcs[modId] = new Dictionary<string, List<MessageHandler>>();
                if (!rpcs[modId].ContainsKey(method.Name)) rpcs[modId][method.Name] = new List<MessageHandler>();
                rpcs[modId][method.Name].Add(new MessageHandler
                {
                    Target = instance,
                    Method = method,
                    Parameters = method.GetParameters(),
                    TakesInfo = method.GetParameters().Length > 0 && method.GetParameters().Last().ParameterType.Name == "RPCInfo",
                    Mask = mask
                });
                registered++;
            }
            return new Token(() => DeregisterNetworkObject(instance, modId, mask));
        }

        /// <summary>
        /// </summary>
        public void DeregisterNetworkObject(object instance, uint modId, int mask = 0)
        {
            if (!rpcs.TryGetValue(modId, out var methods)) return;
            foreach (var method in instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attrs = method.GetCustomAttributes(false).OfType<CustomRPCAttribute>().ToArray();
                if (attrs.Length == 0) continue;
                if (!methods.TryGetValue(method.Name, out var handlers)) continue;
                for (int i = handlers.Count - 1; i >= 0; i--)
                    if (handlers[i].Target == instance && handlers[i].Mask == mask) handlers.RemoveAt(i);
            }
        }

        /// <summary>
        /// </summary>
        public void RPC(uint modId, string methodName, ReliableType reliable, params object[] parameters)
        {
            var msg = BuildMessage(modId, methodName, 0, parameters);
            if (msg == null) return;
            DispatchIncoming(msg, LocalSteamId);
        }

        /// <summary>
        /// </summary>
        public void RPCTarget(uint modId, string methodName, ulong targetSteamId64, ReliableType reliable, params object[] parameters)
        {
            var msg = BuildMessage(modId, methodName, 0, parameters);
            if (msg == null) return;
            if (targetSteamId64 == LocalSteamId) DispatchIncoming(msg, LocalSteamId);
        }

        /// <summary>
        /// </summary>
        public void RPCToHost(uint modId, string methodName, ReliableType reliable, params object[] parameters)
        {
            RPCTarget(modId, methodName, HostSteamId64, reliable, parameters);
        }

        /// <summary>
        /// </summary>
        public void RegisterLobbyDataKey(string key) => lobbyKeys.Add(key);
        /// <summary>
        /// </summary>
        public void SetLobbyData(string key, object value)
        {
            if (!InLobby) { Debug.LogError("Cannot set lobby data when not in lobby."); return; }
            if (!lobbyKeys.Contains(key)) Debug.LogWarning($"Accessing unregistered lobby key {key}");
            lobbyData[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            LobbyDataChanged?.Invoke(new[] { key });
        }
        /// <summary>
        /// </summary>
        public T GetLobbyData<T>(string key)
        {
            if (!InLobby) { Debug.LogError("Cannot get lobby data when not in lobby."); return default!; }
            if (!lobbyKeys.Contains(key)) Debug.LogWarning($"Accessing unregistered lobby key {key}");
            if (!lobbyData.TryGetValue(key, out var v)) return default!;
            try { return (T)Convert.ChangeType(v, typeof(T), System.Globalization.CultureInfo.InvariantCulture); }
            catch { Debug.LogError($"Could not parse lobby data [{key},{v}]"); return default!; }
        }

        /// <summary>
        /// </summary>
        public void RegisterPlayerDataKey(string key) => playerKeys.Add(key);
        /// <summary>
        /// </summary>
        public void SetPlayerData(string key, object value)
        {
            if (!InLobby) { Debug.LogError("Cannot set player data when not in lobby."); return; }
            if (!playerKeys.Contains(key)) Debug.LogWarning($"Accessing unregistered player key {key}");
            perPlayerData[LocalSteamId][key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            PlayerDataChanged?.Invoke(LocalSteamId, new[] { key });
        }
        /// <summary>
        /// </summary>
        public T GetPlayerData<T>(ulong steamId64, string key)
        {
            if (!InLobby) { Debug.LogError("Cannot get player data when not in lobby."); return default!; }
            if (!playerKeys.Contains(key)) Debug.LogWarning($"Accessing unregistered player key {key}");
            if (!perPlayerData.TryGetValue(steamId64, out var dict)) return default!;
            if (!dict.TryGetValue(key, out var v)) return default!;
            try { return (T)Convert.ChangeType(v, typeof(T), System.Globalization.CultureInfo.InvariantCulture); }
            catch { Debug.LogError($"Could not parse player data [{key},{v}]"); return default!; }
        }

        /// <summary>
        /// </summary>
        public void PollReceive()
        {
            // Nothing queued in offline mode.
        }

        Message? BuildMessage(uint modId, string methodName, int mask, object[] parameters)
        {
            try
            {
                var msg = new Message(modId, methodName, mask);
                if (rpcs.TryGetValue(modId, out var methods) && methods.TryGetValue(methodName, out var handlers) && handlers.Count > 0)
                {
                    var handler = handlers[0];
                    var expected = handler.Parameters;
                    int expectedCount = handler.TakesInfo ? expected.Length - 1 : expected.Length;
                    if (expectedCount != parameters.Length)
                        throw new Exception($"Parameter count mismatch: expected {expectedCount}, got {parameters.Length}");
                    for (int i = 0; i < expectedCount; i++)
                    {
                        var t = expected[i].ParameterType;
                        var p = parameters[i] ?? throw new Exception($"Null parameter {i}");
                        if (!t.IsAssignableFrom(p.GetType())) throw new Exception($"Type mismatch {t} vs {p.GetType()}");
                        msg.WriteObject(t, p);
                    }
                }
                else
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) throw new Exception("Null parameter");
                        msg.WriteObject(p.GetType(), p);
                    }
                }
                return msg;
            }
            catch (Exception ex) { Debug.LogError($"BuildMessage failed: {ex}"); return null; }
        }

        void DispatchIncoming(Message message, ulong from)
        {
            if (IncomingValidator != null && !IncomingValidator(message, from)) return;

            if (!rateLimiter.IncomingAllowed()) return;

            if (!rpcs.TryGetValue(message.ModID, out var methods)) { Debug.LogWarning($"No mod {message.ModID}"); return; }
            if (!methods.TryGetValue(message.MethodName, out var handlers)) { Debug.LogWarning($"No method {message.MethodName}"); return; }

            foreach (var handler in handlers.ToArray())
            {
                if (handler.Mask != message.Mask) continue;
                var pi = handler.Parameters;
                int paramCount = handler.TakesInfo ? pi.Length - 1 : pi.Length;
                var callParams = new object[pi.Length];
                for (int i = 0; i < paramCount; i++) callParams[i] = message.ReadObject(pi[i].ParameterType);
                if (handler.TakesInfo)
                {
                    var t = pi[pi.Length - 1].ParameterType;
                    callParams[pi.Length - 1] = CreateRpcInfoInstance(t, from);
                }
                try { handler.Method.Invoke(handler.Target, callParams); }
                catch (Exception ex) { Debug.LogError($"Invoke RPC error: {ex}"); }
            }
        }

        object CreateRpcInfoInstance(Type infoType, ulong from)
        {
            try
            {
                var ci = infoType.GetConstructor(new[] { typeof(ulong) });
                if (ci != null) return ci.Invoke(new object[] { from });
                var p = Activator.CreateInstance(infoType);
                var field = infoType.GetField("SenderSteamID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) field.SetValue(p, from);
                return p!;
            }
            catch { return null!; }
        }

        class SlidingWindowRateLimiter
        {
            readonly int limit; readonly TimeSpan window; readonly Queue<DateTime> q = new();
            public SlidingWindowRateLimiter(int limit, TimeSpan window) { this.limit = limit; this.window = window; }
            public bool IncomingAllowed() { var now = DateTime.UtcNow; while (q.Count > 0 && now - q.Peek() > window) q.Dequeue(); if (q.Count >= limit) return false; q.Enqueue(now); return true; }
        }

        class MessageHandler { public object Target = null!; public MethodInfo Method = null!; public ParameterInfo[] Parameters = null!; public bool TakesInfo; public int Mask; }
    }
}
#endif
