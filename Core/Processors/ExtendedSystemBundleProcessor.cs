using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PEAKLib.Core;
using PEAKLib.Levels.Core;
using Object = UnityEngine.Object;
using static PEAKLib.Levels.Core.ExtendedSegment;

internal class ExtendedSystemBundleProcessor : MonoBehaviour
{
    public string AllowedModId = "PLL";

    private readonly List<PeakBundle> _queued = new();

    private void Awake()
    {
        BundleLoader.OnBundleLoaded += OnBundleLoaded;
        BundleLoader.OnAllBundlesLoaded += OnAllBundlesLoaded;
    }
    private void OnDestroy()
    {
        BundleLoader.OnBundleLoaded -= OnBundleLoaded;
        BundleLoader.OnAllBundlesLoaded -= OnAllBundlesLoaded;
    }

    private void OnBundleLoaded(PeakBundle pb)
    {
        if (pb == null) return;
        _queued.Add(pb);
        StartCoroutine(ProcessBundleCoroutine(pb));
    }

    private void OnAllBundlesLoaded()
    {
        foreach (var pb in _queued.ToArray())
        {
            StartCoroutine(ProcessBundleCoroutine(pb));
        }
    }

    private IEnumerator ProcessBundleCoroutine(PeakBundle pb)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(AllowedModId) && !string.Equals(pb.Mod.Id, AllowedModId, StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            var extendedMods = new List<ExtendedMod>();

            try
            {
                extendedMods.AddRange(pb.Mod.Content.OfType<ExtendedMod>());
            }
            catch {  }

            if (extendedMods.Count == 0)
            {
                var soReq = pb.LoadAllAssetsAsync<ScriptableObject>();
                var allSos = soReq.allAssets.OfType<ScriptableObject>().ToArray();
                foreach (var so in allSos)
                {
                    if (so is ExtendedMod extMod)
                    {
                        extendedMods.Add(extMod);
                    }
                    else
                    {
                        if (string.Equals(so.name, "MOD", StringComparison.OrdinalIgnoreCase) ||
                            so.name.EndsWith("_Mod", StringComparison.OrdinalIgnoreCase))
                        {
                            if (so.GetType().Name.Equals(nameof(ExtendedMod), StringComparison.OrdinalIgnoreCase))
                            {
                                try { extendedMods.Add((ExtendedMod)so); } catch { }
                            }
                        }
                    }
                }
            }

            if (extendedMods.Count == 0)
            {
                Debug.Log($"[ExtendedSystemBundleProcessor] No ExtendedMod found in bundle {pb.Mod.Id}. Skipping.");
                yield break;
            }

            foreach (var emod in extendedMods)
            {
                Debug.Log($"[ExtendedSystemBundleProcessor] Processing ExtendedMod '{emod.ModName}' from bundle {pb.Mod.Id}");
                foreach (var content in emod.ExtendedContents.ToArray())
                {
                    if (content is ExtendedSegment seg)
                    {
                        StartCoroutine(ProcessExtendedSegment(pb, emod, seg));
                    }
                    else
                    {
                        // Extra ExtendedContent
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ExtendedSystemBundleProcessor] Error processing bundle {pb.Mod.Id}: {ex}");
        }
        finally
        {
            _queued.Remove(pb);
        }
    }

    private IEnumerator ProcessExtendedSegment(PeakBundle pb, ExtendedMod emod, ExtendedSegment seg)
    {
        GameObject? biomePrefab = seg.BiomePrefab;

        if (biomePrefab == null)
        {
            string[] names = pb.GetAllAssetNames();
            string candidate = names.FirstOrDefault(n => n.IndexOf(seg.name, StringComparison.OrdinalIgnoreCase) >= 0 && n.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));

            if (candidate == null)
            {
                candidate = names.FirstOrDefault(n => n.IndexOf("biome", StringComparison.OrdinalIgnoreCase) >= 0 && n.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));
            }

            if (candidate != null)
            {
                var req = pb.LoadAssetAsync<GameObject>(candidate);
                while (!req.isDone) yield return null;
                biomePrefab = req.asset as GameObject;
            }
            else
            {
                Debug.LogWarning($"[ExtendedSystemBundleProcessor] Could not find prefab asset name in bundle for ExtendedSegment '{seg.name}' in mod '{emod.ModName}'");
            }
        }

        if (biomePrefab == null)
        {
            Debug.LogWarning($"[ExtendedSystemBundleProcessor] Biome prefab null for segment '{seg.name}' (mod {emod.ModName}). Skipping segment.");
            yield break;
        }

        GameObject? campfireChild = null;
        GameObject? segmentChild = null;

        var root = biomePrefab.transform;
        var campfireTransform = root.FindDeep(seg.CampfireChildName);
        var segmentTransform = root.FindDeep(seg.SegmentChildName);

        if (campfireTransform != null) campfireChild = campfireTransform.gameObject;
        if (segmentTransform != null) segmentChild = segmentTransform.gameObject;

        if (campfireChild == null || segmentChild == null)
        {
            var placeholders = biomePrefab.GetComponentsInChildren<Placeholder>(true);
            foreach (var ph in placeholders)
            {
                if (string.Equals(ph.Role, "campfire", StringComparison.OrdinalIgnoreCase) && campfireChild == null)
                    campfireChild = ph.gameObject;
                if (string.Equals(ph.Role, "segment", StringComparison.OrdinalIgnoreCase) && segmentChild == null)
                    segmentChild = ph.gameObject;
            }
        }

        if (campfireChild == null || segmentChild == null)
        {
            foreach (Transform t in root.GetAllChildrenRecursive())
            {
                if (campfireChild == null && t.name.IndexOf("campfire", StringComparison.OrdinalIgnoreCase) >= 0) campfireChild = t.gameObject;
                if (segmentChild == null && t.name.IndexOf("segment", StringComparison.OrdinalIgnoreCase) >= 0) segmentChild = t.gameObject;
                if (campfireChild != null && segmentChild != null) break;
            }
        }

        var info = new BiomeInfo
        {
            ModName = emod.ModName,
            SegmentName = seg.name,
            BiomePrefab = biomePrefab,
            CampfireObject = campfireChild!,
            SegmentObject = segmentChild!
        };

        Debug.Log($"[ExtendedSystemBundleProcessor] Found biome for '{seg.name}' (mod {emod.ModName}): prefab='{biomePrefab.name}', campfire='{info.CampfireObject?.name ?? "null"}', segment='{info.SegmentObject?.name ?? "null"}'");

        if (info.CampfireObject != null)
        {
            var ph = info.CampfireObject.GetComponent<Placeholder>();
            if (ph != null)
            {
                Debug.Log($"[ExtendedSystemBundleProcessor] Campfire placeholder role='{ph.Role}' spawnable='{ph.SpawnableName}' options='{ph.Options}'");
            }
        }
        if (info.SegmentObject != null)
        {
            var ph = info.SegmentObject.GetComponent<Placeholder>();
            if (ph != null)
            {
                Debug.Log($"[ExtendedSystemBundleProcessor] Segment placeholder role='{ph.Role}' spawnable='{ph.SpawnableName}' options='{ph.Options}'");
            }
        }

        // TODO: register found prefabs in prefab pool, spawnable registry etc...

        yield break;
    }
}