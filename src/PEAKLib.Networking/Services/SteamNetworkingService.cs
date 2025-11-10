#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine;
using PEAKLib.Networking.Modules;

namespace PEAKLib.Networking.Services
{
    /// <summary>
    /// </summary>
    public class SteamNetworkingService : INetworkingService
    {
        const int CHANNEL = 120;
        const int MAX_IN_MESSAGES = 500;
        static IntPtr[] inMessages = new IntPtr[MAX_IN_MESSAGES];

        /// <summary>
        /// </summary>
        public bool IsInitialized { get; private set; } = false;
        /// <summary>
        /// </summary>
        public bool InLobby { get; private set; } = false;
        /// <summary>
        /// </summary>
        public ulong HostSteamId64
        {
            get
            {
                if (Lobby == CSteamID.Nil) return 0UL;
                var owner = SteamMatchmaking.GetLobbyOwner(Lobby);
                if (owner == CSteamID.Nil) return 0UL;
                return owner.m_SteamID;
            }
        }
        /// <summary>
        /// </summary>
        public string HostIdString
        {
            get
            {
                if (Lobby == CSteamID.Nil) return string.Empty;
                var owner = SteamMatchmaking.GetLobbyOwner(Lobby);
                return owner == CSteamID.Nil ? string.Empty : owner.ToString();
            }
        }

        /// <summary>
        /// </summary>
        public CSteamID Lobby { get; private set; } = CSteamID.Nil;
        private CSteamID[] players = Array.Empty<CSteamID>();

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

        private static readonly List<string> lobbyDataKeys = new();
        private static readonly List<string> playerDataKeys = new();
        private static readonly Dictionary<CSteamID, Dictionary<string, string>> lastPlayerData = new();
        private static readonly Dictionary<string, string> lastLobbyData = new();

        Callback<LobbyEnter_t>? cbLobbyEnter;
        Callback<LobbyCreated_t>? cbLobbyCreated;
        Callback<LobbyChatUpdate_t>? cbLobbyChatUpdate;
        Callback<LobbyDataUpdate_t>? cbLobbyDataUpdate;

        readonly Dictionary<uint, Dictionary<string, List<MessageHandler>>> rpcs = new();

        readonly Queue<QueuedSend> highQueue = new();
        readonly Queue<QueuedSend> normalQueue = new();
        readonly Queue<QueuedSend> lowQueue = new();
        readonly object queueLock = new();

        readonly Dictionary<(ulong target, ulong msgId), UnackedMessage> unacked = new();
        readonly object unackedLock = new();
        TimeSpan ackTimeout = TimeSpan.FromSeconds(1.2);
        int maxRetransmitAttempts = 5;

        private long _nextMessageId = 0;
        private ulong NextMessageId() => (ulong)Interlocked.Increment(ref _nextMessageId);
        readonly Dictionary<uint, ulong> outgoingSequencePerMod = new();
        readonly Dictionary<ulong, Dictionary<uint, ulong>> lastSeenSequence = new();

        readonly Dictionary<ulong, SlidingWindowRateLimiter> rateLimiters = new();

        readonly Dictionary<ulong, byte[]> perPeerSymmetricKey = new();
        byte[]? globalSharedSecret = null;
        HMACSHA256? globalHmac = null;

        readonly Dictionary<uint, Func<byte[], byte[]>> modSigners = new();
        readonly Dictionary<uint, RSAParameters> modPublicKeys = new();

        readonly Dictionary<ulong, HandshakeState> handshakeStates = new();

        const byte FRAG_FLAG = 0x1;
        const byte COMPRESSED_FLAG = 0x2;
        const byte HMAC_FLAG = 0x4;
        const byte SIGN_FLAG = 0x8;
        const byte ACK_FLAG = 0x10;

        RSACryptoServiceProvider? LocalRsa;

        /// <summary>
        /// </summary>
        public SteamNetworkingService() { }

        /// <summary>
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                Message.MaxSize = (int)Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;
            }
            catch { }

            cbLobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            cbLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            cbLobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            cbLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);

            LocalRsa = new RSACryptoServiceProvider(2048);

            IsInitialized = true;
            NetworkingPlugin.Log.LogInfo("SteamNetworkingService initialized");
        }

        /// <summary>
        /// </summary>
        public void Shutdown()
        {
            cbLobbyEnter = null;
            cbLobbyCreated = null;
            cbLobbyChatUpdate = null;
            cbLobbyDataUpdate = null;

            rpcs.Clear();
            unacked.Clear();
            handshakeStates.Clear();
            perPeerSymmetricKey.Clear();
            globalHmac?.Dispose();
            globalHmac = null;
            LocalRsa?.Dispose();
            LocalRsa = null;

            IsInitialized = false;
            NetworkingPlugin.Log.LogInfo("SteamNetworkingService shutdown");
        }

        /// <summary>
        /// </summary>
        public void CreateLobby(int maxPlayers = 8) => SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, maxPlayers);
        /// <summary>
        /// </summary>
        public void JoinLobby(ulong lobbySteamId64) => SteamMatchmaking.JoinLobby(new CSteamID(lobbySteamId64));
        /// <summary>
        /// </summary>
        public void LeaveLobby() => OnLobbyLeftInternal();
        /// <summary>
        /// </summary>
        public void InviteToLobby(ulong steamId64) => SteamMatchmaking.InviteUserToLobby(Lobby, new CSteamID(steamId64));

        void OnLobbyEnter(LobbyEnter_t param)
        {
            NetworkingPlugin.Log.LogDebug($"LobbyEnter {param.m_ulSteamIDLobby}");
            Lobby = new CSteamID(param.m_ulSteamIDLobby);
            InLobby = true;
            RefreshPlayerList();
            LobbyEntered?.Invoke();
        }

        void OnLobbyCreated(LobbyCreated_t param)
        {
            NetworkingPlugin.Log.LogDebug($"LobbyCreated: {param.m_eResult}");
            if (param.m_eResult == EResult.k_EResultOK)
            {
                Lobby = new CSteamID(param.m_ulSteamIDLobby);
                InLobby = true;
                RefreshPlayerList();
                LobbyCreated?.Invoke();
            }
            else
            {
                NetworkingPlugin.Log.LogError($"Lobby creation failed: {param.m_eResult}");
            }
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t param)
        {
            var player = new CSteamID(param.m_ulSteamIDUserChanged);
            RefreshPlayerList();

            var change = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
            switch (change)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    PlayerEntered?.Invoke(player.m_SteamID);
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    PlayerLeft?.Invoke(player.m_SteamID);
                    break;
            }
        }

        internal void OnLobbyLeftInternal()
        {
            lastLobbyData.Clear();
            lastPlayerData.Clear();
            InLobby = false;
            LobbyLeft?.Invoke();
        }

        void RefreshPlayerList()
        {
            players = new CSteamID[SteamMatchmaking.GetNumLobbyMembers(Lobby)];
            for (int i = 0; i < players.Length; i++)
                players[i] = SteamMatchmaking.GetLobbyMemberByIndex(Lobby, i);
        }

        void OnLobbyDataUpdate(LobbyDataUpdate_t param)
        {
            if (!InLobby) return;
            if (param.m_ulSteamIDLobby != Lobby.m_SteamID) return;

            if (param.m_ulSteamIDLobby == param.m_ulSteamIDMember)
            {
                var changed = new List<string>();
                foreach (var key in lobbyDataKeys)
                {
                    var data = SteamMatchmaking.GetLobbyData(Lobby, key);
                    if (!lastLobbyData.TryGetValue(key, out var prev) || prev != data)
                    {
                        changed.Add(key);
                        lastLobbyData[key] = data;
                    }
                }
                if (changed.Count > 0) LobbyDataChanged?.Invoke(changed.ToArray());
            }
            else
            {
                var player = new CSteamID(param.m_ulSteamIDMember);
                if (!lastPlayerData.ContainsKey(player)) lastPlayerData[player] = new Dictionary<string, string>();
                var changed = new List<string>();
                foreach (var key in playerDataKeys)
                {
                    var data = SteamMatchmaking.GetLobbyMemberData(Lobby, player, key);
                    if (!lastPlayerData[player].TryGetValue(key, out var prev) || prev != data)
                    {
                        changed.Add(key);
                        lastPlayerData[player][key] = data;
                    }
                }
                if (changed.Count > 0) PlayerDataChanged?.Invoke(player.m_SteamID, changed.ToArray());
            }
        }

        /// <summary>
        /// </summary>
        public void RegisterLobbyDataKey(string key)
        {
            if (lobbyDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Lobby key {key} already registered");
            else lobbyDataKeys.Add(key);
        }

        /// <summary>
        /// </summary>
        public void SetLobbyData(string key, object value)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Cannot set lobby data when not in lobby."); return; }
            if (!lobbyDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Accessing unregistered lobby key '{key}'.");
            SteamMatchmaking.SetLobbyData(Lobby, key, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// </summary>
        public T GetLobbyData<T>(string key)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Cannot get lobby data when not in lobby."); return default(T)!; }
            if (!lobbyDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Accessing unregistered lobby key '{key}'.");
            string v = SteamMatchmaking.GetLobbyData(Lobby, key);
            if (string.IsNullOrEmpty(v)) return default(T)!;
            try { return (T)Convert.ChangeType(v, typeof(T), System.Globalization.CultureInfo.InvariantCulture); }
            catch { NetworkingPlugin.Log.LogError($"Could not parse lobby data [{key},{v}] as {typeof(T).Name}"); return default(T)!; }
        }

        /// <summary>
        /// </summary>
        public void RegisterPlayerDataKey(string key)
        {
            if (playerDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Player key {key} already registered");
            else playerDataKeys.Add(key);
        }

        /// <summary>
        /// </summary>
        public void SetPlayerData(string key, object value)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Cannot set player data when not in lobby."); return; }
            if (!playerDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Accessing unregistered player key '{key}'.");
            SteamMatchmaking.SetLobbyMemberData(Lobby, key, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// </summary>
        public T GetPlayerData<T>(ulong steamId64, string key)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Cannot get player data when not in lobby."); return default(T)!; }
            if (!playerDataKeys.Contains(key)) NetworkingPlugin.Log.LogWarning($"Accessing unregistered player key '{key}'.");
            var player = new CSteamID(steamId64);
            string v = SteamMatchmaking.GetLobbyMemberData(Lobby, player, key);
            if (string.IsNullOrEmpty(v)) return default(T)!;
            try { return (T)Convert.ChangeType(v, typeof(T), System.Globalization.CultureInfo.InvariantCulture); }
            catch { NetworkingPlugin.Log.LogError($"Could not parse player data [{key},{v}] as {typeof(T).Name}"); return default(T)!; }
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

                var mh = new MessageHandler
                {
                    Target = instance,
                    Method = method,
                    Parameters = method.GetParameters(),
                    TakesInfo = method.GetParameters().Length > 0 && method.GetParameters().Last().ParameterType.Name == "RPCInfo",
                    Mask = mask
                };
                rpcs[modId][method.Name].Add(mh);
                registered++;
            }

            NetworkingPlugin.Log.LogInfo($"Registered {registered} RPCs for mod {modId} on {instance}");
            return new RegistrationToken(this, instance, modId, mask);
        }

        /// <summary>
        /// </summary>
        public void DeregisterNetworkObject(object instance, uint modId, int mask = 0)
        {
            if (!rpcs.TryGetValue(modId, out var methods))
            {
                NetworkingPlugin.Log.LogWarning($"No RPCs for mod {modId}");
                return;
            }

            int removed = 0;
            foreach (var method in instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attrs = method.GetCustomAttributes(false).OfType<CustomRPCAttribute>().ToArray();
                if (attrs.Length == 0) continue;
                if (!methods.TryGetValue(method.Name, out var handlers)) continue;

                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    if (handlers[i].Target == instance && handlers[i].Mask == mask)
                    {
                        handlers.RemoveAt(i);
                        removed++;
                    }
                }
            }

            NetworkingPlugin.Log.LogInfo($"Deregistered {removed} RPCs for mod {modId} on {instance}");
        }

        sealed class RegistrationToken : IDisposable
        {
            SteamNetworkingService svc;
            object instance;
            uint modId;
            int mask;
            bool disposed;
            public RegistrationToken(SteamNetworkingService svc, object instance, uint modId, int mask)
            {
                this.svc = svc; this.instance = instance; this.modId = modId; this.mask = mask;
            }
            public void Dispose()
            {
                if (disposed) return;
                svc.DeregisterNetworkObject(instance, modId, mask);
                disposed = true;
            }
        }

        /// <summary>
        /// </summary>
        public void RPC(uint modId, string methodName, ReliableType reliable, params object[] parameters)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("RPC called while not in lobby"); return; }
            var msg = BuildMessage(modId, methodName, 0, parameters);
            if (msg == null) return;

            var framedTemplate = BuildFramedBytesWithMeta(msg, modId, reliable);

            foreach (var p in players)
            {
                if (p == SteamUser.GetSteamID()) continue;
                var framed = framedTemplate;
                EnqueueOrSend(framed, p, reliable, DeterminePriority(modId, methodName));
            }

            InvokeLocalMessage(new Message(msg.ToArray()), SteamUser.GetSteamID());
        }

        /// <summary>
        /// </summary>
        public void RPCTarget(uint modId, string methodName, ulong targetSteamId64, ReliableType reliable, params object[] parameters)
        {
            RPCTarget(modId, methodName, new CSteamID(targetSteamId64), reliable, parameters);
        }

        /// <summary>
        /// </summary>
        public void RPCTarget(uint modId, string methodName, CSteamID target, ReliableType reliable, params object[] parameters)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Cannot RPC target when not in lobby"); return; }
            var msg = BuildMessage(modId, methodName, 0, parameters);
            if (msg == null) return;
            var framed = BuildFramedBytesWithMeta(msg, modId, reliable);
            EnqueueOrSend(framed, target, reliable, DeterminePriority(modId, methodName));
        }

        /// <summary>
        /// </summary>
        public void RPCToHost(uint modId, string methodName, ReliableType reliable, params object[] parameters)
        {
            if (!InLobby) { NetworkingPlugin.Log.LogError("Not in lobby"); return; }
            var host = SteamMatchmaking.GetLobbyOwner(Lobby);
            if (host == CSteamID.Nil) { NetworkingPlugin.Log.LogError("No host set"); return; }
            RPCTarget(modId, methodName, host, reliable, parameters);
        }

        Priority DeterminePriority(uint modId, string methodName)
        {
            var lower = methodName.ToLowerInvariant();
            if (lower.Contains("admin") || lower.Contains("control") || lower.Contains("critical") || lower.Contains("sync")) return Priority.High;
            return Priority.Normal;
        }

        void EnqueueOrSend(byte[] framed, CSteamID target, ReliableType reliable, Priority p)
        {
            var rl = GetOrCreateRateLimiter(target.m_SteamID);
            if (!rl.Allowed())
            {
                NetworkingPlugin.Log.LogWarning($"Rate limit: dropping send to {target}");
                return;
            }

            if (p == Priority.High)
            {
                SendWithPossibleAck(framed, target, reliable);
                return;
            }

            lock (queueLock)
            {
                var q = p == Priority.Normal ? normalQueue : lowQueue;
                q.Enqueue(new QueuedSend { Framed = framed, Target = target, Reliable = reliable, Enqueued = DateTime.UtcNow });
            }
        }

        void FlushQueues(int maxPerFrame = 8)
        {
            int sent = 0;
            while (sent < maxPerFrame)
            {
                QueuedSend item = null!;
                lock (queueLock)
                {
                    if (normalQueue.Count > 0) item = normalQueue.Dequeue();
                    else if (lowQueue.Count > 0) item = lowQueue.Dequeue();
                    else break;
                }
                if (item != null)
                {
                    SendWithPossibleAck(item.Framed, item.Target, item.Reliable);
                    sent++;
                }
            }
        }

        byte[] BuildFramedBytesWithMeta(Message msg, uint modId, ReliableType reliable)
        {
            var payload = msg.ToArray();
            bool compress = payload.Length > 1024;
            if (compress) payload = msg.CompressPayload(); // Uncertain of this messes up everything or not, did not test this.

            byte flags = 0;
            if (compress) flags |= COMPRESSED_FLAG;
            if (globalHmac != null) flags |= HMAC_FLAG;
            if (modSigners.ContainsKey(modId)) flags |= SIGN_FLAG;

            ulong seq;
            lock (outgoingSequencePerMod)
            {
                if (!outgoingSequencePerMod.TryGetValue(modId, out var cur)) cur = 0;
                seq = ++cur;
                outgoingSequencePerMod[modId] = cur;
            }

            ulong msgId = NextMessageId();

            using var ms = new MemoryStream();
            ms.WriteByte(flags);
            ms.Write(BitConverter.GetBytes(msgId), 0, 8);
            ms.Write(BitConverter.GetBytes(seq), 0, 8);
            ms.Write(BitConverter.GetBytes(1), 0, 4); // total fragments
            ms.Write(BitConverter.GetBytes(0), 0, 4); // index
            ms.Write(payload, 0, payload.Length);

            byte[] headerAndPayload = ms.ToArray();

            if (modSigners.TryGetValue(modId, out var signer))
            {
                var sig = signer(headerAndPayload);
                using var ms2 = new MemoryStream();
                ms2.Write(headerAndPayload, 0, headerAndPayload.Length);
                var len = (ushort)sig.Length;
                ms2.Write(BitConverter.GetBytes(len), 0, 2);
                ms2.Write(sig, 0, sig.Length);
                headerAndPayload = ms2.ToArray();
            }

            if (globalHmac != null)
            {
                var mac = globalHmac!.ComputeHash(headerAndPayload);
                using var ms3 = new MemoryStream();
                ms3.Write(headerAndPayload, 0, headerAndPayload.Length);
                ms3.Write(mac, 0, mac.Length);
                headerAndPayload = ms3.ToArray();
            }

            if (reliable == ReliableType.Reliable) headerAndPayload[0] = (byte)(headerAndPayload[0] | ACK_FLAG);

            return headerAndPayload;
        }

        void SendWithPossibleAck(byte[] framed, CSteamID target, ReliableType reliable)
        {
            if (perPeerSymmetricKey.TryGetValue(target.m_SteamID, out var sym))
            {
                using var h = new HMACSHA256(sym);
                var mac = h.ComputeHash(framed);
                using var ms = new MemoryStream();
                ms.Write(framed, 0, framed.Length);
                ms.Write(mac, 0, mac.Length);
                framed = ms.ToArray();
                framed[0] = (byte)(framed[0] | HMAC_FLAG);
            }

            bool requestAck = (framed[0] & ACK_FLAG) != 0;

            if (requestAck)
            {
                var msgId = BitConverter.ToUInt64(framed, 1);
                var key = (target.m_SteamID, msgId);
                lock (unackedLock)
                {
                    unacked[key] = new UnackedMessage { Framed = framed, Target = target, Reliable = reliable, LastSent = DateTime.UtcNow, Attempts = 1 };
                }
            }

            SendBytes(framed, target, reliable);
        }

        void SendBytes(byte[] data, CSteamID target, ReliableType reliable)
        {
            if (data.Length > Message.MaxSize)
            {
                NetworkingPlugin.Log.LogError($"Send length {data.Length} exceeds Message.MaxSize {Message.MaxSize}");
                return;
            }

            if (target == SteamUser.GetSteamID())
            {
                var m = new Message(data);
                InvokeLocalMessage(m, SteamUser.GetSteamID());
                return;
            }

            var id = new SteamNetworkingIdentity();
            id.SetSteamID(target);
            GCHandle pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr p = pinned.AddrOfPinnedObject();

            int flags = Constants.k_nSteamNetworkingSend_AutoRestartBrokenSession;
            switch (reliable)
            {
                case ReliableType.Unreliable: flags |= Constants.k_nSteamNetworkingSend_Unreliable; break;
                case ReliableType.Reliable: flags |= Constants.k_nSteamNetworkingSend_Reliable; break;
                case ReliableType.UnreliableNoDelay: flags |= Constants.k_nSteamNetworkingSend_UnreliableNoDelay; break;
            }

            var res = SteamNetworkingMessages.SendMessageToUser(ref id, p, (uint)data.Length, flags, CHANNEL);
            if (res != EResult.k_EResultOK)
            {
                NetworkingPlugin.Log.LogError($"SendMessageToUser failed: {res} to {target}");
            }

            pinned.Free();
        }

        /// <summary>
        /// </summary>
        public void PollReceive()
        {
            FlushQueues();
            RetransmitUnacked();
            ReceiveMessages();
        }

        void RetransmitUnacked()
        {
            var toRetransmit = new List<UnackedMessage>();
            lock (unackedLock)
            {
                var now = DateTime.UtcNow;
                var keys = unacked.Keys.ToArray();
                foreach (var key in keys)
                {
                    var info = unacked[key];
                    if (now - info.LastSent > ackTimeout)
                    {
                        if (info.Attempts >= maxRetransmitAttempts)
                        {
                            unacked.Remove(key);
                            NetworkingPlugin.Log.LogWarning($"Message {key.msgId} to {key.target} dropped after {info.Attempts} attempts.");
                        }
                        else
                        {
                            info.Attempts++;
                            info.LastSent = now;
                            unacked[key] = info;
                            toRetransmit.Add(info);
                        }
                    }
                }
            }

            foreach (var item in toRetransmit)
            {
                SendBytes(item.Framed, item.Target, item.Reliable);
            }
        }

        void ReceiveMessages()
        {
            int count = SteamNetworkingMessages.ReceiveMessagesOnChannel(CHANNEL, inMessages, MAX_IN_MESSAGES);
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                IntPtr outPtr = inMessages[i];
                SteamNetworkingMessage_t steamMsg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(outPtr);
                int size = (int)steamMsg.m_cbSize;
                if (size <= 0) { SteamNetworkingMessage_t.Release(outPtr); continue; }
                if (size > Message.MaxSize) { NetworkingPlugin.Log.LogError($"Incoming message size {size} > max {Message.MaxSize}"); SteamNetworkingMessage_t.Release(outPtr); continue; }

                CSteamID sender = steamMsg.m_identityPeer.GetSteamID();
                if (sender == CSteamID.Nil) { SteamNetworkingMessage_t.Release(outPtr); continue; }

                byte[] bytes = new byte[size];
                Marshal.Copy(steamMsg.m_pData, bytes, 0, size);

                try { ProcessIncomingFrame(bytes, sender); }
                catch (Exception ex) { NetworkingPlugin.Log.LogError($"ProcessIncomingFrame exception: {ex}"); }

                SteamNetworkingMessage_t.Release(outPtr);
            }
        }

        void ProcessIncomingFrame(byte[] frame, CSteamID sender)
        {
            using var ms = new MemoryStream(frame);
            int flags = ms.ReadByte();
            bool compressed = (flags & COMPRESSED_FLAG) != 0;
            bool hasHmac = (flags & HMAC_FLAG) != 0;
            bool hasSign = (flags & SIGN_FLAG) != 0;
            bool requiresAck = (flags & ACK_FLAG) != 0;

            byte[] msgIdBytes = new byte[8]; ms.Read(msgIdBytes, 0, 8);
            ulong msgId = BitConverter.ToUInt64(msgIdBytes, 0);

            byte[] seqBytes = new byte[8]; ms.Read(seqBytes, 0, 8);
            ulong seq = BitConverter.ToUInt64(seqBytes, 0);

            byte[] totalBytes = new byte[4]; ms.Read(totalBytes, 0, 4);
            int total = BitConverter.ToInt32(totalBytes, 0);

            byte[] indexBytes = new byte[4]; ms.Read(indexBytes, 0, 4);
            int index = BitConverter.ToInt32(indexBytes, 0);

            int payloadLen = (int)(ms.Length - ms.Position);
            byte[] payloadWithOptionalMacAndSig = new byte[payloadLen];
            ms.Read(payloadWithOptionalMacAndSig, 0, payloadLen);

            byte[] payload = payloadWithOptionalMacAndSig;

            if (perPeerSymmetricKey.TryGetValue(sender.m_SteamID, out var peerSym))
            {
                if (payloadWithOptionalMacAndSig.Length < 32) { NetworkingPlugin.Log.LogWarning("Invalid HMAC payload"); return; }
                int dataLen = payloadWithOptionalMacAndSig.Length - 32;
                var dataOnly = new byte[dataLen];
                Array.Copy(payloadWithOptionalMacAndSig, 0, dataOnly, 0, dataLen);
                var mac = new byte[32];
                Array.Copy(payloadWithOptionalMacAndSig, dataLen, mac, 0, 32);
                using var h = new HMACSHA256(peerSym);
                var computed = h.ComputeHash(dataOnly);
                if (!computed.SequenceEqual(mac)) { NetworkingPlugin.Log.LogWarning("Per-peer HMAC mismatch; dropping"); return; }
                payload = dataOnly;
            }
            else if (hasHmac && globalHmac != null)
            {
                if (payloadWithOptionalMacAndSig.Length < 32) { NetworkingPlugin.Log.LogWarning("Invalid global HMAC payload"); return; }
                int dataLen = payloadWithOptionalMacAndSig.Length - 32;
                var dataOnly = new byte[dataLen];
                Array.Copy(payloadWithOptionalMacAndSig, 0, dataOnly, 0, dataLen);
                var mac = new byte[32];
                Array.Copy(payloadWithOptionalMacAndSig, dataLen, mac, 0, 32);
                var computed = globalHmac!.ComputeHash(dataOnly);
                if (!computed.SequenceEqual(mac)) { NetworkingPlugin.Log.LogWarning("Global HMAC mismatch; dropping"); return; }
                payload = dataOnly;
            }

            if (compressed)
            {
                try { payload = Message.DecompressPayload(payload); }
                catch { NetworkingPlugin.Log.LogWarning("Decompression failed"); return; }
            }

            var message = new Message(payload);

            if (message.ModID == 0)
            {
                HandleInternalMessage(message, sender, msgId, seq, requiresAck);
                if (requiresAck) SendAckToSender(sender, msgId);
                return;
            }

            var sender64 = sender.m_SteamID;
            var rl = GetOrCreateRateLimiter(sender64);
            if (!rl.IncomingAllowed())
            {
                NetworkingPlugin.Log.LogWarning($"Rate limit: dropping incoming from {sender64}");
                return;
            }

            if (!CheckAndUpdateSequence(sender64, message.ModID, seq))
            {
                NetworkingPlugin.Log.LogDebug($"Dropped replay/out-of-order seq {seq} from {sender64} for mod {message.ModID}");
                return;
            }

            if (requiresAck) SendAckToSender(sender, msgId);

            if (IncomingValidator != null && !IncomingValidator(message, sender64))
            {
                NetworkingPlugin.Log.LogDebug($"Incoming message from {sender64} rejected by validator");
                return;
            }

            DispatchIncoming(message, sender);
        }

        void SendAckToSender(CSteamID sender, ulong msgId)
        {
            var ackMsg = new Message(0u, "PEAK_INTERNAL_ACK", 0);
            ackMsg.WriteULong(msgId);
            var framed = BuildFramedBytesWithMeta(ackMsg, 0, ReliableType.Reliable);
            SendBytes(framed, sender, ReliableType.Reliable);

            var key = (sender.m_SteamID, msgId);
            lock (unackedLock)
            {
                if (unacked.ContainsKey(key)) unacked.Remove(key);
            }
        }

        void HandleInternalMessage(Message message, CSteamID sender, ulong msgId, ulong seq, bool requiresAck)
        {
            try
            {
                switch (message.MethodName)
                {
                    case "PEAK_INTERNAL_HANDSHAKE_PUBKEY":
                        {
                            string pub = message.ReadString();
                            string nonce = message.ReadString();
                            StartHandshakeReply(sender, pub, nonce);
                            break;
                        }
                    case "PEAK_INTERNAL_HANDSHAKE_SECRET":
                        {
                            var enc = (byte[])message.ReadObject(typeof(byte[]));
                            string initiator = message.ReadString();
                            CompleteHandshakeReceiver(sender, enc, initiator);
                            break;
                        }
                    case "PEAK_INTERNAL_HANDSHAKE_CONFIRM":
                        {
                            string initiator = message.ReadString();
                            var confirm = (byte[])message.ReadObject(typeof(byte[]));
                            CompleteHandshakeInitiator(sender, initiator, confirm);
                            break;
                        }
                    case "PEAK_INTERNAL_ACK":
                        {
                            ulong ackId = message.ReadULong();
                            var key = (sender.m_SteamID, ackId);
                            lock (unackedLock) { if (unacked.ContainsKey(key)) unacked.Remove(key); }
                            break;
                        }
                    default:
                        NetworkingPlugin.Log.LogWarning($"Unknown internal method {message.MethodName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                NetworkingPlugin.Log.LogError($"HandleInternalMessage error: {ex}");
            }
        }

        bool CheckAndUpdateSequence(ulong sender64, uint modId, ulong seq)
        {
            lock (lastSeenSequence)
            {
                if (!lastSeenSequence.TryGetValue(sender64, out var dict)) dict = lastSeenSequence[sender64] = new Dictionary<uint, ulong>();
                if (!dict.TryGetValue(modId, out var last)) last = 0;
                if (seq <= last) return false;
                dict[modId] = seq;
                return true;
            }
        }

        void DispatchIncoming(Message message, CSteamID sender)
        {
            if (!rpcs.TryGetValue(message.ModID, out var methods))
            {
                NetworkingPlugin.Log.LogWarning($"Dropping message for unknown mod {message.ModID}");
                return;
            }

            if (!methods.TryGetValue(message.MethodName, out var handlers))
            {
                NetworkingPlugin.Log.LogWarning($"Dropping message for method {message.MethodName} not registered for {message.ModID}");
                return;
            }

            bool invoked = false;
            foreach (var handler in handlers)
            {
                if (handler.Mask != message.Mask) continue;
                var paramInfos = handler.Parameters;
                int paramCount = handler.TakesInfo ? paramInfos.Length - 1 : paramInfos.Length;
                var callParams = new object[paramInfos.Length];

                for (int i = 0; i < paramCount; i++)
                {
                    var t = paramInfos[i].ParameterType;
                    callParams[i] = message.ReadObject(t);
                }

                if (handler.TakesInfo)
                {
                    var infoType = paramInfos[paramInfos.Length - 1].ParameterType;
                    object infoObj = CreateRpcInfoInstance(infoType, sender);
                    callParams[paramInfos.Length - 1] = infoObj;
                }

                try
                {
                    handler.Method.Invoke(handler.Target, callParams);
                    invoked = true;
                }
                catch (Exception ex)
                {
                    NetworkingPlugin.Log.LogError($"RPC invoke error {message.ModID}:{message.MethodName}: {ex}");
                }
            }

            if (!invoked)
                NetworkingPlugin.Log.LogWarning($"No handler matched for {message.ModID}:{message.MethodName}");
        }

        object CreateRpcInfoInstance(Type infoType, CSteamID sender)
        {
            try
            {
                var ci = infoType.GetConstructor(new Type[] { typeof(CSteamID) });
                if (ci != null) return ci.Invoke(new object[] { sender });

                var ci2 = infoType.GetConstructor(new Type[] { typeof(ulong) });
                if (ci2 != null) return ci2.Invoke(new object[] { sender.m_SteamID });

                var paramless = infoType.GetConstructor(Type.EmptyTypes);
                if (paramless != null)
                {
                    var obj = paramless.Invoke(null);
                    var f = infoType.GetField("SenderSteamID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (f != null) f.SetValue(obj, sender);
                    var f2 = infoType.GetField("Sender", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (f2 != null) f2.SetValue(obj, sender);
                    return obj;
                }
            }
            catch { }
            return null!;
        }

        class HandshakeState { public string? PeerPub; public string LocalNonce = string.Empty; public byte[]? Sym; public bool Completed = false; }

        /// <summary>
        /// </summary>
        public void StartHandshake(CSteamID target)
        {
            if (LocalRsa == null) return;
            var pub = SerializeRsaPublicKey(LocalRsa);
            var rng = RandomNumberGenerator.Create();
            var nonceBytes = new byte[16]; rng.GetBytes(nonceBytes);
            var nonce = Convert.ToBase64String(nonceBytes);

            handshakeStates[target.m_SteamID] = new HandshakeState { PeerPub = null, LocalNonce = nonce, Completed = false };

            var m = new Message(0u, "PEAK_INTERNAL_HANDSHAKE_PUBKEY", 0);
            m.WriteString(pub);
            m.WriteString(nonce);
            var framed = BuildFramedBytesWithMeta(m, 0, ReliableType.Reliable);
            SendBytes(framed, target, ReliableType.Reliable);
        }

        void StartHandshakeReply(CSteamID sender, string peerPubKeySerialized, string peerNonce)
        {
            if (LocalRsa == null) return;
            var state = handshakeStates.ContainsKey(sender.m_SteamID) ? handshakeStates[sender.m_SteamID] : new HandshakeState();
            state.PeerPub = peerPubKeySerialized;
            if (string.IsNullOrEmpty(state.LocalNonce))
            {
                var rng = RandomNumberGenerator.Create();
                var localNonceBytes = new byte[16]; rng.GetBytes(localNonceBytes);
                state.LocalNonce = Convert.ToBase64String(localNonceBytes);
                handshakeStates[sender.m_SteamID] = state;

                var myPub = SerializeRsaPublicKey(LocalRsa);
                var m = new Message(0u, "PEAK_INTERNAL_HANDSHAKE_PUBKEY", 0);
                m.WriteString(myPub);
                m.WriteString(state.LocalNonce);
                var framed = BuildFramedBytesWithMeta(m, 0, ReliableType.Reliable);
                SendBytes(framed, sender, ReliableType.Reliable);
                return;
            }

            if (!state.Completed && !string.IsNullOrEmpty(state.LocalNonce) && !string.IsNullOrEmpty(state.PeerPub))
            {
                var rng = RandomNumberGenerator.Create();
                var sym = new byte[32]; rng.GetBytes(sym);

                var rsaPeer = new RSACryptoServiceProvider();
                var rsaParams = DeserializeRsaPublicKey(state.PeerPub!);
                rsaPeer.ImportParameters(rsaParams);
                var enc = rsaPeer.Encrypt(sym, false);

                var m2 = new Message(0u, "PEAK_INTERNAL_HANDSHAKE_SECRET", 0);
                m2.WriteBytes(enc);
                m2.WriteString(state.LocalNonce);
                var framed2 = BuildFramedBytesWithMeta(m2, 0, ReliableType.Reliable);
                SendBytes(framed2, sender, ReliableType.Reliable);

                state.Sym = sym; state.Completed = true;
                handshakeStates[sender.m_SteamID] = state;
                perPeerSymmetricKey[sender.m_SteamID] = sym;
            }
        }

        void CompleteHandshakeReceiver(CSteamID sender, byte[] encSecret, string initiatorNonce)
        {
            if (LocalRsa == null) return;
            try
            {
                var sym = LocalRsa.Decrypt(encSecret, false);
                var state = handshakeStates.ContainsKey(sender.m_SteamID) ? handshakeStates[sender.m_SteamID] : new HandshakeState();
                state.Sym = sym; state.Completed = true; handshakeStates[sender.m_SteamID] = state;
                perPeerSymmetricKey[sender.m_SteamID] = sym;

                var confirm = HmacSha256Raw(sym, Encoding.UTF8.GetBytes(initiatorNonce));
                var m = new Message(0u, "PEAK_INTERNAL_HANDSHAKE_CONFIRM", 0);
                m.WriteString(initiatorNonce);
                m.WriteBytes(confirm);
                var framed = BuildFramedBytesWithMeta(m, 0, ReliableType.Reliable);
                SendBytes(framed, sender, ReliableType.Reliable);
            }
            catch (Exception ex) { NetworkingPlugin.Log.LogError($"Handshake decryption error: {ex}"); }
        }

        void CompleteHandshakeInitiator(CSteamID sender, string initiatorNonce, byte[] confirmHmac)
        {
            if (!handshakeStates.TryGetValue(sender.m_SteamID, out var state) || state.Sym == null)
            {
                NetworkingPlugin.Log.LogWarning("Handshake confirm received but no state");
                return;
            }

            var expected = HmacSha256Raw(state.Sym, Encoding.UTF8.GetBytes(initiatorNonce));
            if (!expected.SequenceEqual(confirmHmac))
            {
                NetworkingPlugin.Log.LogWarning("Handshake confirm HMAC mismatch");
                return;
            }

            state.Completed = true;
            handshakeStates[sender.m_SteamID] = state;
            perPeerSymmetricKey[sender.m_SteamID] = state.Sym!;
        }

        static byte[] HmacSha256Raw(byte[] key, byte[] payload)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(payload);
        }

        static string SerializeRsaPublicKey(RSACryptoServiceProvider rsa)
        {
            var parms = rsa.ExportParameters(false);
            var mod = Convert.ToBase64String(parms.Modulus ?? Array.Empty<byte>());
            var exp = Convert.ToBase64String(parms.Exponent ?? Array.Empty<byte>());
            return $"{mod}:{exp}";
        }

        static RSAParameters DeserializeRsaPublicKey(string s)
        {
            var parts = s.Split(':');
            var mod = Convert.FromBase64String(parts[0]);
            var exp = Convert.FromBase64String(parts[1]);
            return new RSAParameters { Modulus = mod, Exponent = exp };
        }

        /// <summary>
        /// </summary>
        public void SetSharedSecret(byte[]? secret)
        {
            if (secret == null) { globalSharedSecret = null; globalHmac?.Dispose(); globalHmac = null; return; }
            globalSharedSecret = (byte[])secret.Clone();
            globalHmac?.Dispose();
            globalHmac = new HMACSHA256(globalSharedSecret);
        }

        /// <summary>
        /// </summary>
        public void RegisterModSigner(uint modId, Func<byte[], byte[]> signerDelegate) => modSigners[modId] = signerDelegate;
        /// <summary>
        /// </summary>
        public void RegisterModPublicKey(uint modId, RSAParameters pub) => modPublicKeys[modId] = pub;

        void InvokeLocalMessage(Message message, CSteamID localSender)
        {
            ulong id = localSender.m_SteamID;
            if (IncomingValidator != null && !IncomingValidator(message, id)) return;

            if (!rpcs.TryGetValue(message.ModID, out var methods)) return;
            if (!methods.TryGetValue(message.MethodName, out var handlers)) return;

            foreach (var handler in handlers)
            {
                if (handler.Mask != message.Mask) continue;
                var paramInfos = handler.Parameters;
                int paramCount = handler.TakesInfo ? paramInfos.Length - 1 : paramInfos.Length;
                var callParams = new object[paramInfos.Length];

                for (int i = 0; i < paramCount; i++)
                    callParams[i] = message.ReadObject(paramInfos[i].ParameterType);

                if (handler.TakesInfo)
                {
                    var infoType = paramInfos[paramInfos.Length - 1].ParameterType;
                    callParams[paramInfos.Length - 1] = CreateRpcInfoInstance(infoType, localSender);
                }

                try { handler.Method.Invoke(handler.Target, callParams); }
                catch (Exception ex) { NetworkingPlugin.Log.LogError($"Local invoke error: {ex}"); }
            }
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
                        if (!t.IsAssignableFrom(p.GetType()))
                            throw new Exception($"Parameter {i} type mismatch: expected {t}, got {p.GetType()}");
                        msg.WriteObject(t, p);
                    }
                }
                else
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) throw new Exception("Null parameter cannot be serialized without signature");
                        msg.WriteObject(p.GetType(), p);
                    }
                }

                if (msg.Length() > Message.MaxSize * 16)
                {
                    NetworkingPlugin.Log.LogError("Message exceeds maximum allowed overall size.");
                    return null;
                }

                return msg;
            }
            catch (Exception ex)
            {
                NetworkingPlugin.Log.LogError($"BuildMessage failed: {ex}");
                return null;
            }
        }

        SlidingWindowRateLimiter GetOrCreateRateLimiter(ulong steam64)
        {
            lock (rateLimiters)
            {
                if (!rateLimiters.TryGetValue(steam64, out var rl))
                {
                    rl = new SlidingWindowRateLimiter(100, TimeSpan.FromSeconds(1));
                    rateLimiters[steam64] = rl;
                }
                return rl;
            }
        }

        class SlidingWindowRateLimiter
        {
            readonly int limit;
            readonly TimeSpan window;
            readonly Queue<DateTime> q = new();
            public SlidingWindowRateLimiter(int limit, TimeSpan window) { this.limit = limit; this.window = window; }
            public bool Allowed()
            {
                var now = DateTime.UtcNow;
                while (q.Count > 0 && now - q.Peek() > window) q.Dequeue();
                if (q.Count >= limit) return false;
                q.Enqueue(now);
                return true;
            }
            public bool IncomingAllowed() => Allowed();
        }

        enum Priority { High = 0, Normal = 1, Low = 2 }
        class QueuedSend { public byte[] Framed = null!; public CSteamID Target; public ReliableType Reliable; public DateTime Enqueued; }
        class UnackedMessage { public byte[] Framed = null!; public CSteamID Target; public ReliableType Reliable; public DateTime LastSent; public int Attempts; public ulong msgId => BitConverter.ToUInt64(Framed, 1); }
        class MessageHandler { public object Target = null!; public MethodInfo Method = null!; public ParameterInfo[] Parameters = null!; public bool TakesInfo; public int Mask; }

        static byte[] HmacSha256RawStatic(byte[] key, byte[] payload) { using var h = new HMACSHA256(key); return h.ComputeHash(payload); }
    }
}
#endif
