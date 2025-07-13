using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Tests;
using System.IO;
using System.Reflection;
using UnityEngine;
using PEAKLib.Stats;
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

    private void Awake()
    {
        Instance = this;
        Definition = ModDefinition.GetOrCreate(Info.Metadata);

        this.LoadBundleWithName(
            "testball.peakbundle",
            bundle =>
            {
                var testBallPrefab = bundle.LoadAsset<GameObject>("TestBall.prefab");
                // attach behavior
                testBallPrefab.AddComponent<TestBall>();
                var action = testBallPrefab.AddComponent<Action_TestBallRecolor>();
                action.OnCastFinished = true;
                bundle.Mod.RegisterContent();
            }
        );

        // status effect test: sun during daytime
        string sunBundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "peaklibtest.peakbundle.sunstatus");
        var sunBundle = AssetBundle.LoadFromFile(sunBundlePath);
        var sunTex = sunBundle.LoadAsset<Texture2D>("IC_Heat");
        Status sunStatus = new Status()
        {
            Name = "Sun",
            Color = Color.Lerp(Color.red, Color.yellow, 0.65f),
            MaxAmount = 0.1f,
            AllowClear = false,

            // these are ignored because we use Update
            ReductionCooldown = 1f,
            ReductionPerSecond = 0f,

            Icon = Sprite.Create(sunTex, new Rect(0, 0, sunTex.width, sunTex.height), new Vector2(0.5f, 0.5f)),

            SFX = new SFX_Instance
            {
                clips = [sunBundle.LoadAsset<AudioClip>("status")],
                settings = new(),
            },

            Update = (self, status) => {
                // this DOES apply in airport for testing purposes
                // check !self.m_inAirport to prevent this

                // only apply to shore or tropics
                bool notReachedAlpine = (bool)Singleton<MountainProgressHandler>.Instance &&
                    Singleton<MountainProgressHandler>.Instance.maxProgressPointReached < 2;
                // calculate how close to noon it is
                float sun = 0f;
                if (DayNightManager.instance != null && DayNightManager.instance.isDay > 0.5f)
                {
                    float dayStart = DayNightManager.instance.dayStart / 24f;
                    float dayEnd = DayNightManager.instance.dayEnd / 24f;
                    float time = DayNightManager.instance.timeOfDayNormalized;
                    sun = 2f * Mathf.Min(Mathf.Abs(time - dayStart), Mathf.Abs(time - dayEnd)) / Mathf.Abs(dayStart - dayEnd);
                    sun = Mathf.Max(0.01f, sun * status.MaxAmount);
                }
                // apply scaling heat status depending on how close to noon it is
                self.SetStatus(status.Type, sun);
            },
        };
        new StatusContent(sunStatus).Register(Definition);

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
