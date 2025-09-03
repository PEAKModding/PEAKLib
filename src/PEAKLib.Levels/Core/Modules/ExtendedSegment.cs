using System;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    [CreateAssetMenu(fileName = "ExtendedSegment", menuName = "PEAK/ExtendedContent/ExtendedSegment", order = 12)]
    internal class ExtendedSegment : ExtendedContent<GameObject>
    {
        public int Index = 0;
        public bool Replace = false;
        public bool IsVariant = false;

        [Tooltip("The biome's name, will display in the Hero title.")]
        public string Biome = string.Empty;

        [Tooltip("Mod's unique identifier")]
        public string ID = string.Empty;

        [Tooltip("The Biome prefab (top-level). This prefab should contain a 'Segment' child and a 'Campfire' child (or similar).")]
        public GameObject? BiomePrefab;

        [Tooltip("Child name inside BiomePrefab that is the playable segment.")]
        public string SegmentChildName = "Segment";

        [Tooltip("Child name inside BiomePrefab that is the campfire root.")]
        public string CampfireChildName = "Campfire";

        [Serializable]
        public class SpawnMapping
        {
            public string SpawnerMarker = "";
            public string SpawnableName = "";
        }
        public List<SpawnMapping> SpawnMappings = new List<SpawnMapping>();
        [NonSerialized] public Dictionary<string, GameObject> ResolvedSpawnables = new Dictionary<string, GameObject>();

        internal override void Register(ExtendedMod Mod)
        {
            Mod.RegisterExtendedContentInternal(this);
        }
    }

    internal class BiomeInfo
    {
        public string? ModName;
        public string? SegmentName;
        public GameObject? BiomePrefab;
        public GameObject? CampfireObject;
        public GameObject? SegmentObject;
    }
}
