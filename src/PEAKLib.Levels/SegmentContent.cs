using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PEAKLib.Core;
using PEAKLib.Levels.Core;
using UnityEngine;

namespace PEAKLib.Levels;

// TODO: Implement this.

/// <summary>
/// A registerable PEAKLib level segment.
/// </summary>
public class SegmentContent : IContent<SegmentContent>
{
    /// <summary>
    /// The name of this segment.
    /// </summary>
    public string Name => Biome;
    public int Index = 0;
    public bool Replace = false;
    public bool IsVariant = false;

    [Tooltip("The biome's name, will display in the Hero title.")]
    public string Biome = string.Empty;

    [Tooltip(
        "The Biome prefab (top-level). This prefab should contain a 'Segment' child and a 'Campfire' child (or similar)."
    )]
    public GameObject BiomePrefab = null!;

    [Tooltip("Child name inside BiomePrefab that is the playable segment.")]
    public string SegmentChildName = "Segment";

    [Tooltip("Child name inside BiomePrefab that is the campfire root.")]
    public string CampfireChildName = "Campfire";
    internal GameObject? SegmentObject;
    internal GameObject? CampfireObject;

    [Serializable]
    public class SpawnMapping
    {
        public string SpawnerMarker = "";
        public string SpawnableName = "";
    }

    public List<SpawnMapping> SpawnMappings = new List<SpawnMapping>();

    [NonSerialized]
    public Dictionary<string, GameObject> ResolvedSpawnables = new Dictionary<string, GameObject>();

    internal static List<RegisteredContent<SegmentContent>> s_RegisteredSegments = [];

    /// <inheritdoc/>
    public RegisteredContent<SegmentContent> Register(ModDefinition owner)
    {
        ProcessExtendedSegment(owner);
        var registered = ContentRegistry.Register(this, owner);
        s_RegisteredSegments.Add(registered);
        return registered;
    }

    /// <inheritdoc/>
    IRegisteredContent IContent.Register(ModDefinition owner) => Register(owner);

    /// <inheritdoc/>
    public IContent Resolve() => this;

    void ProcessExtendedSegment(ModDefinition mod)
    {
        GameObject? biomePrefab = BiomePrefab;
        var seg = this;

        if (biomePrefab == null)
        {
            Debug.LogWarning(
                $"[ExtendedSystemBundleProcessor] Biome prefab null for segment '{seg.Name}' (mod {mod.Id}). Skipping segment."
            );
            return;
        }

        GameObject? campfireChild = null;
        GameObject? segmentChild = null;

        var root = biomePrefab.transform;
        var campfireTransform = root.FindDeep(seg.CampfireChildName);
        var segmentTransform = root.FindDeep(seg.SegmentChildName);

        if (campfireTransform != null)
            campfireChild = campfireTransform.gameObject;
        if (segmentTransform != null)
            segmentChild = segmentTransform.gameObject;

        if (campfireChild == null || segmentChild == null)
        {
            var placeholders = biomePrefab.GetComponentsInChildren<Placeholder>(true);
            foreach (var ph in placeholders)
            {
                if (
                    string.Equals(ph.Role, "campfire", StringComparison.OrdinalIgnoreCase)
                    && campfireChild == null
                )
                    campfireChild = ph.gameObject;
                if (
                    string.Equals(ph.Role, "segment", StringComparison.OrdinalIgnoreCase)
                    && segmentChild == null
                )
                    segmentChild = ph.gameObject;
            }
        }

        if (campfireChild == null || segmentChild == null)
        {
            foreach (Transform t in root.GetAllChildrenRecursive())
            {
                if (
                    campfireChild == null
                    && t.name.Contains("campfire", StringComparison.OrdinalIgnoreCase)
                )
                    campfireChild = t.gameObject;
                if (
                    segmentChild == null
                    && t.name.Contains("segment", StringComparison.OrdinalIgnoreCase)
                )
                    segmentChild = t.gameObject;
                if (campfireChild != null && segmentChild != null)
                    break;
            }
        }

        CampfireObject = campfireChild;
        SegmentObject = segmentChild;

        Debug.Log(
            $"[ExtendedSystemBundleProcessor] Found biome for '{seg.Name}' (mod {mod.Id}): prefab='{biomePrefab.name}', campfire='{CampfireObject?.name ?? "null"}', segment='{SegmentObject?.name ?? "null"}'"
        );

        if (CampfireObject != null)
        {
            var ph = CampfireObject.GetComponent<Placeholder>();
            if (ph != null)
            {
                Debug.Log(
                    $"[ExtendedSystemBundleProcessor] Campfire placeholder role='{ph.Role}' spawnable='{ph.SpawnableName}' options='{ph.Options}'"
                );
            }
        }
        if (SegmentObject != null)
        {
            var ph = SegmentObject.GetComponent<Placeholder>();
            if (ph != null)
            {
                Debug.Log(
                    $"[ExtendedSystemBundleProcessor] Segment placeholder role='{ph.Role}' spawnable='{ph.SpawnableName}' options='{ph.Options}'"
                );
            }
        }

        RegisterPlaceholdersFromPrefab(mod);

        // TODO: register found prefabs in prefab pool, spawnable registry etc...

        return;
    }

    void RegisterPlaceholdersFromPrefab(ModDefinition mod)
    {
        var placeholders = BiomePrefab.GetComponentsInChildren<Placeholder>(true);
        foreach (var ph in placeholders)
        {
            if (!string.IsNullOrEmpty(ph.SpawnableName))
            {
                var spawnName = ph.SpawnableName.Trim();
                var assetPath = ph.gameObject.name;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var bundlePath = mod.Id;
                    PatchedContent.SpawnableRegistry.Registry[spawnName] =
                        new PatchedContent.SpawnableEntry(bundlePath, assetPath);
                    Debug.Log(
                        $"[SpawnableRegistrar] Registered Placeholder spawn '{spawnName}' => {bundlePath} :: {assetPath} (role='{ph.Role}')"
                    );
                }
            }
        }

        var luggages = BiomePrefab.GetComponentsInChildren<LuggageMarker>(true);
        foreach (var lm in luggages)
        {
            if (!string.IsNullOrEmpty(lm.LuggageKey))
            {
                var key = lm.LuggageKey.Trim();
                var assetPath = lm.gameObject.name;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    PatchedContent.SpawnableRegistry.Registry[key] =
                        new PatchedContent.SpawnableEntry(mod.Id, assetPath);
                    Debug.Log(
                        $"[SpawnableRegistrar] Registered Luggage '{key}' => {mod.Id} :: {assetPath}"
                    );
                }
            }
        }
    }
}
