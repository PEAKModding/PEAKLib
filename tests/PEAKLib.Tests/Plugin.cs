using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Stats;
using PEAKLib.Tests;
using UnityEngine;
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
        MonolithPrefab = bundle.LoadAsset<GameObject>("Monolith.prefab");
        MonolithPrefab.AddComponent<AcceptItem_Monolith>();
        MonolithPrefab.GetComponentInChildren<Renderer>().material.shader = Shader.Find("W/Peak_Standard");
    }
}
