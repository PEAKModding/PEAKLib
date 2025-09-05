using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal class PackSystem
    {
        public static void ApplyPacksToMapHandler(object __instance)
        {
            try
            {
                if (__instance == null) return;
                var map = __instance as MapHandler;
                if (map == null)
                {
                    Debug.LogWarning("[PackSystem] ApplyPacksToMapHandler: __instance is not MapHandler");
                    return;
                }

                bool anyDeferred = false;
                foreach (var registeredContent in SegmentContent.s_RegisteredSegments)
                {
                    if (registeredContent.Content.BiomePrefab != null)
                    {
                        TryApplySegmentToMap(map, registeredContent);
                    }
                    else
                    {
                        anyDeferred = true;
                    }
                }

                if (anyDeferred)
                {
                    if (LevelsPlugin.Instance != null)
                    {
                        LevelsPlugin.Instance.StartCoroutine(DeferredApplyCoroutine(map));
                    }
                    else
                    {
                        Debug.LogWarning("[PackSystem] LevelsPlugin.Instance is null - deferred apply cancelled.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PackSystem] ApplyPacksToMapHandler error: {ex}");
            }
        }

        public static void TryApplySegmentToMap(
            MapHandler map,
            RegisteredContent<SegmentContent> registeredSegment
        )
        {
            var seg = registeredSegment.Content;
            try
            {
                int idx = seg.Index;
                /*if (idx < 0 || map.segments == null || idx >= map.segments.Length)
                {
                    Debug.LogWarning($"[PackSystem] ExtendedSegment '{seg.name}' index {idx} out of range.");
                    return;
                }*/

                GameObject? prefab = seg.BiomePrefab;
                if (prefab == null)
                {
                    Debug.LogError($"[PackSystem] No prefab available for '{seg.Name}' yet; skipping (will be applied later).");
                    return;
                }

                Transform parentContainer = map.globalParent != null ? map.globalParent : map.transform;
                var instance = UnityEngine.Object.Instantiate(prefab, parentContainer);
                instance.name = $"{seg.Name}_BiomeInstance";

                bool isCurrent = (Segment)idx == map.GetCurrentSegment();
                instance.SetActive(isCurrent);

                var ms = map.segments[idx];
                var segType = typeof(MapHandler.MapSegment);
                var parentField = segType.GetField("_segmentParent", BindingFlags.NonPublic | BindingFlags.Instance);
                var campfireField = segType.GetField("_segmentCampfire", BindingFlags.NonPublic | BindingFlags.Instance);

                if (parentField != null)
                    parentField.SetValue(ms, instance);
                else
                    Debug.LogWarning("[PackSystem] MapSegment._segmentParent not found via reflection.");

                GameObject? campfire = null;
                if (!string.IsNullOrEmpty(seg.CampfireChildName))
                {
                    var tf = GeneralHelper.FindDeepByName(instance.transform, seg.CampfireChildName);
                    if (tf != null) campfire = tf.gameObject;
                }

                if (campfire == null)
                {
                    var phs = instance.GetComponentsInChildren<Placeholder>(true);
                    foreach (var p in phs)
                    {
                        if (string.Equals(p.Role, "campfire", StringComparison.OrdinalIgnoreCase)) { campfire = p.gameObject; break; }
                    }
                }

                if (campfireField != null)
                    campfireField.SetValue(ms, campfire);

                Debug.Log($"[PackSystem] Applied ExtendedSegment '{seg.Name}' to MapHandler.segments[{idx}]");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PatchHooks] TryApplySegmentToMap error for '{seg?.Name}': {ex}");
            }
        }

        static IEnumerator DeferredApplyCoroutine(MapHandler map)
        {
            bool fired = false;
            Action? onAll = () => fired = true;
            BundleLoader.OnAllBundlesLoaded += onAll;
            yield return new WaitUntil(() => fired);

            BundleLoader.OnAllBundlesLoaded -= onAll;

            foreach (var registeredContent in SegmentContent.s_RegisteredSegments)
            {
                TryApplySegmentToMap(map, registeredContent);
            }

            try
            {
                var detect = typeof(MapHandler).GetMethod("DetectBiomes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                detect?.Invoke(map, null);
            }
            catch (Exception e) { Debug.LogWarning($"[PackSystem] DeferredApplyCoroutine: DetectBiomes failed: {e}"); }
        }
    }
}
