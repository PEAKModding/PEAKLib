﻿using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Stats;
using PEAKLib.Tests;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;

namespace PEAKLib.Items;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
[BepInDependency(ItemsPlugin.Id)]
[BepInDependency(StatsPlugin.Id)]
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
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
}
