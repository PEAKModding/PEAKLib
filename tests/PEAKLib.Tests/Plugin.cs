using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Items;
using PEAKLib.Stats;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;
using PEAKLib.Networking.Services;
using PEAKLib.Networking.Modules;
using System.Text;
using System.Reflection;
using System;
using PEAKLib.Networking;

namespace PEAKLib.Tests;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
[BepInDependency(ItemsPlugin.Id)]
[BepInDependency(StatsPlugin.Id)]
[BepInDependency(NetworkingPlugin.Id)]
public partial class TestsPlugin : BaseUnityPlugin
{
    public static ModDefinition Definition { get; set; } = null!;
    public static TestsPlugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Name);

    internal static CharacterAfflictions.STATUSTYPE SpikyStatus;

    GameObject? monolithPrefab;
    Vector3 monolithPosition = new Vector3(-0.8f, 4.1f, -368.4f);
    bool modifiedBingBong = false;
    const ushort bingBongID = 13;

    INetworkingService? netSvc = null;
    IDisposable? netRegToken = null;
    const uint NET_TEST_MOD_ID = 0xF00DBABE;

    private void Awake()
    {
        Instance = this;
        Definition = ModDefinition.GetOrCreate(Info.Metadata);

        this.LoadBundleWithName("testball.peakbundle", InitTestBall);

        this.LoadBundleWithName(
            "teststatus.peakbundle",
            statusBundle =>
            {
                InitSpikyStatus(statusBundle);
                InitSunStatus(statusBundle);
            }
        );

        this.LoadBundleWithName(
            "testmonolith.peakbundle",
            monolithBundle =>
            {
                InitMonolith(monolithBundle);
            }
        );

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded!");

        try
        {
            InitializeNetworkingTests();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Networking test init failed: {ex.Message}");
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        try
        {
            netRegToken?.Dispose();
        }
        catch { }

        try
        {
            netSvc?.Shutdown();
        }
        catch { }
    }

    // test ball for multiplayer item data
    private void InitTestBall(PeakBundle bundle)
    {
        var testBallPrefab = bundle.LoadAsset<GameObject>("TestBall.prefab");
        // attach behavior
        testBallPrefab.AddComponent<TestBall>();
        var action = testBallPrefab.AddComponent<Action_TestBallRecolor>();
        action.OnCastFinished = true;

        bundle.Mod.RegisterContent();
    }

    // basic triggered status effect test: spiky
    // triggered when we recolor the test ball
    private void InitSpikyStatus(PeakBundle bundle)
    {
        var spikyTex = bundle.LoadAsset<Texture2D>("IC_Spiky");
        var statusSFX = bundle.LoadAsset<AudioClip>("status");
        Status spikyStatus = new Status()
        {
            Name = "Spiky",
            Color = Color.Lerp(Color.black, Color.white, 0.5f),
            MaxAmount = 2f,
            AllowClear = true,

            ReductionCooldown = 1.5f,
            ReductionPerSecond = 0.01f,

            Icon = Sprite.Create(
                spikyTex,
                new Rect(0, 0, spikyTex.width, spikyTex.height),
                new Vector2(0.5f, 0.5f)
            ),

            SFX = new SFX_Instance { clips = [statusSFX], settings = new() },
        };
        new StatusContent(spikyStatus).Register(Definition);
        SpikyStatus = spikyStatus.Type;
    }

    // automatically-applied status effect test: sun during daytime
    private void InitSunStatus(PeakBundle bundle)
    {
        var sunTex = bundle.LoadAsset<Texture2D>("IC_Heat");
        var statusSFX = bundle.LoadAsset<AudioClip>("status");
        Status sunStatus = new Status()
        {
            Name = "Sun",
            Color = Color.Lerp(Color.red, Color.yellow, 0.65f),
            MaxAmount = 0.1f,
            AllowClear = false,

            // these are ignored because we use Update
            ReductionCooldown = 1f,
            ReductionPerSecond = 0f,

            Icon = Sprite.Create(
                sunTex,
                new Rect(0, 0, sunTex.width, sunTex.height),
                new Vector2(0.5f, 0.5f)
            ),

            SFX = new SFX_Instance { clips = [statusSFX], settings = new() },

            Update = (self, status) =>
            {
                // only apply to shore or tropics
                bool notReachedAlpine =
                    (bool)Singleton<MountainProgressHandler>.Instance
                    && Singleton<MountainProgressHandler>.Instance.maxProgressPointReached < 2;
                // calculate how close to noon it is
                float sun = 0f;
                if (
                    !self.m_inAirport
                    && DayNightManager.instance != null
                    && DayNightManager.instance.isDay > 0.5f
                )
                {
                    float dayStart = DayNightManager.instance.dayStart / 24f;
                    float dayEnd = DayNightManager.instance.dayEnd / 24f;
                    float time = DayNightManager.instance.timeOfDayNormalized;
                    sun =
                        2f
                        * Mathf.Min(Mathf.Abs(time - dayStart), Mathf.Abs(time - dayEnd))
                        / Mathf.Abs(dayStart - dayEnd);
                    sun = Mathf.Max(0.01f, sun * status.MaxAmount);
                }
                // apply scaling heat status depending on how close to noon it is
                self.SetStatus(status.Type, sun);
            },
        };
        new StatusContent(sunStatus).Register(Definition);
    }

    // Monolith that you can offer items to in exchange for bonus stamina
    private void InitMonolith(PeakBundle bundle)
    {
        monolithPrefab = bundle.LoadAsset<GameObject>("Monolith.prefab");
        monolithPrefab.AddComponent<AcceptItem_Monolith>();
        monolithPrefab.GetComponentInChildren<Renderer>().material.shader = Shader.Find(
            "W/Peak_Standard"
        );
    }

    // spawn the Monolith at game start and modify the bingbong to be feedable
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // check for game start: Load single scene, scene name is Level_X,
        // and game is connected
        Match match = new Regex(@"^Level_(\d+)$").Match(scene.name);
        if (
            mode == LoadSceneMode.Single
            && match.Success
            && int.TryParse(match.Groups[1].Value, out _)
            && PhotonNetwork.IsConnected
            && PhotonNetwork.InRoom
        )
        {
            Log.LogInfo("Game Start detected, spawning Monolith");
            GameObject go = Instantiate(monolithPrefab)!;
            go.transform.position = monolithPosition;
        }
        // modify bingbong if found in ItemDatabase
        if (!modifiedBingBong && ItemDatabase.TryGetItem(bingBongID, out Item item) && item != null)
        {
            Log.LogInfo("Modifying BingBong to accept items");
            item.gameObject.AddComponent<AcceptItem_BingBong>();
            item.gameObject.AddComponent<AcceptItem_BingBong2>();
            modifiedBingBong = true;
        }
    }

    // 
    private void InitializeNetworkingTests()
    {
        if (Steamworks.SteamAPI.IsSteamRunning())
        {
            netSvc = new SteamNetworkingService();
            Log.LogInfo("Using SteamNetworkingService (Steam runtime detected).");
        }
        else
        {
            netSvc = new OfflineNetworkingService();
            Log.LogInfo("Using OfflineNetworkingService for tests.");
        }

        netSvc.LobbyCreated += () => Log.LogInfo("Event: LobbyCreated");
        netSvc.LobbyEntered += () => Log.LogInfo("Event: LobbyEntered");
        netSvc.LobbyLeft += () => Log.LogInfo("Event: LobbyLeft");
        netSvc.PlayerEntered += (u) => Log.LogInfo($"Event: PlayerEntered {u}");
        netSvc.PlayerLeft += (u) => Log.LogInfo($"Event: PlayerLeft {u}");
        netSvc.LobbyDataChanged += (keys) => Log.LogInfo($"Event: LobbyDataChanged: {string.Join(',', keys)}");
        netSvc.PlayerDataChanged += (steam64, keys) => Log.LogInfo($"Event: PlayerDataChanged {steam64}: {string.Join(',', keys)}");

        // example incoming validator: drop RPCs named "DropMe"
        netSvc.IncomingValidator = (msg, steam64) =>
        {
            if (msg.MethodName == "DropMe")
            {
                Log.LogInfo($"IncomingValidator dropping {msg.MethodName} from {steam64}");
                return false;
            }
            return true;
        };

        netSvc.Initialize();

        // register keys used by tests
        netSvc.RegisterLobbyDataKey("TEST_LOBBY_KEY");
        netSvc.RegisterPlayerDataKey("TEST_PLAYER_KEY");

        // register RPCs on this instance for all test methods tagged [CustomRPC]
        try
        {
            netRegToken = netSvc.RegisterNetworkObject(this, NET_TEST_MOD_ID, 0);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to register network object: {ex.Message}");
        }

        // attempt to set shared secret via reflection [optional]
        try
        {
            var mi = netSvc.GetType().GetMethod("SetSharedSecret", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null)
            {
                mi.Invoke(netSvc, new object[] { Encoding.UTF8.GetBytes("test-shared-secret-32-bytes-123456") });
                Log.LogInfo("Set shared secret on networking service (reflection)");
            }
        }
        catch { }

        // create a test lobby
        try
        {
            netSvc.CreateLobby(4);
            Log.LogInfo("Called CreateLobby(4)");
        }
        catch (Exception ex)
        {
            Log.LogWarning($"CreateLobby failed: {ex.Message}");
        }

        // run a handful of RPCs and data ops to exercise the service
        try
        {
            netSvc.RPC(NET_TEST_MOD_ID, "BroadcastTest", ReliableType.Reliable, 123, "hello all");
            netSvc.RPCTarget(NET_TEST_MOD_ID, "TargetTest", netSvc.HostSteamId64 != 0 ? netSvc.HostSteamId64 : 0UL, ReliableType.Unreliable, "hello target");
            netSvc.RPCToHost(NET_TEST_MOD_ID, "ToHostTest", ReliableType.Reliable, "to-host");
            netSvc.RPC(NET_TEST_MOD_ID, "DropMe", ReliableType.Unreliable, "please drop");

            if (netSvc.InLobby)
            {
                netSvc.SetLobbyData("TEST_LOBBY_KEY", "hello-lobby");
                if (netSvc.HostSteamId64 != 0)
                {
                    netSvc.SetPlayerData("TEST_PLAYER_KEY", netSvc.HostSteamId64.ToString());
                    var lv = netSvc.GetLobbyData<string>("TEST_LOBBY_KEY");
                    Log.LogInfo($"GetLobbyData => {lv}");
                    var pv = netSvc.GetPlayerData<string>(netSvc.HostSteamId64, "TEST_PLAYER_KEY");
                    Log.LogInfo($"GetPlayerData => {pv}");
                }
            }

            // spam to test rate limiting
            for (int i = 0; i < 200; i++)
            {
                netSvc.RPC(NET_TEST_MOD_ID, "SpamTest", ReliableType.Unreliable, i);
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Networking test RPC/data ops failed: {ex.Message}");
        }
    }

    private float _netTick = 0f;
    private int _spamCount = 0;
    private void Update()
    {
        // poll networking each frame
        try
        {
            netSvc?.PollReceive();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Networking PollReceive error: {ex.Message}");
        }

        _netTick += Time.deltaTime;
        if (_netTick > 5f)
        {
            _netTick = 0f;
            try
            {
                // re-register cycle test
                netRegToken?.Dispose();
                netRegToken = netSvc?.RegisterNetworkObject(this, NET_TEST_MOD_ID, 0);
                Log.LogInfo("Networking tests: re-registered RPCs");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Re-register error: {ex.Message}");
            }
        }

        if (_spamCount++ == 0)
        {
            try { netSvc?.RPC(NET_TEST_MOD_ID, "ReliablePing", ReliableType.Reliable, "ping"); } catch { }
        }
    }

    // 
    [CustomRPC]
    void BroadcastTest(int n, string text)
    {
        Log.LogInfo($"RPC.BroadcastTest invoked: {n} / {text}");
    }

    [CustomRPC]
    void TargetTest(string message)
    {
        Log.LogInfo($"RPC.TargetTest invoked -> {message}");
    }

    [CustomRPC]
    void ToHostTest(string message)
    {
        Log.LogInfo($"RPC.ToHostTest invoked -> {message}");
    }

    [CustomRPC]
    void DropMe(string reason)
    {
        Log.LogError("DropMe invoked unexpectedly!");
    }

    [CustomRPC]
    void SpamTest(int i)
    {
        if (i % 50 == 0) Log.LogInfo($"SpamTest got {i}");
    }

    [CustomRPC]
    void ReliablePing(string tag)
    {
        Log.LogInfo($"ReliablePing received: {tag}");
        try { netSvc?.RPC(NET_TEST_MOD_ID, "ReliablePong", ReliableType.Reliable, "pong-from-test"); } catch { }
    }

    [CustomRPC]
    void ReliablePong(string tag)
    {
        Log.LogInfo($"ReliablePong: {tag}");
    }
}
