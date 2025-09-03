using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    internal static class FogHelpers
    {
        public static void CreateFogOriginFromPlaceholder(GameObject go, string? placeholderOptions)
        {
            if (go == null) return;
            var fogOriginType = ReflectionHelpers.FindType("FogSphereOrigin") ?? ReflectionHelpers.FindType("FogOrigin") ?? ReflectionHelpers.FindType("FogSphereOriginStub");
            if (fogOriginType == null)
            {
                Debug.LogWarning("[FogHelper] FogSphereOrigin type not found. Skipping fog origin creation.");
                return;
            }
            var comp = go.AddComponent(fogOriginType);
            if (comp == null)
            {
                Debug.LogWarning("[FogHelper] failed to AddComponent FogSphereOrigin on object " + go.name);
                return;
            }
            if (!string.IsNullOrWhiteSpace(placeholderOptions))
            {
                try
                {
                    var jo = JObject.Parse(placeholderOptions);
                    if (jo.TryGetValue("size", StringComparison.OrdinalIgnoreCase, out var s))
                        ReflectionHelpers.SetMemberValue(comp, "size", (float)s.Value<double>());

                    if (jo.TryGetValue("moveOnHeight", StringComparison.OrdinalIgnoreCase, out var h))
                        ReflectionHelpers.SetMemberValue(comp, "moveOnHeight", (float)h.Value<double>());

                    if (jo.TryGetValue("moveOnForward", StringComparison.OrdinalIgnoreCase, out var fwd))
                        ReflectionHelpers.SetMemberValue(comp, "moveOnForward", (float)fwd.Value<double>());

                    if (jo.TryGetValue("disableFog", StringComparison.OrdinalIgnoreCase, out var d))
                        ReflectionHelpers.SetMemberValue(comp, "disableFog", (bool)d.Value<bool>());
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[FogHelper] Failed to parse fog placeholder options: " + ex);
                }
            }

            Debug.Log($"[FogHelper] Created FogSphereOrigin on {go.name}");
        }
    }
}
