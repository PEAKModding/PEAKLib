using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PEAKLib.Core;
using PEAKLib.Levels.Core;

internal static class SpawnableRegistrar
{
    public static void RegisterExtendedSpawnablesFromMod(PeakBundle pb, ExtendedMod emod)
    {
        if (pb == null || emod == null) return;

        foreach (var content in emod.ExtendedContents)
        {
            if (content is ExtendedSpawnable spawn)
            {
                var spawnName = spawn.SpawnableName?.Trim();
                if (string.IsNullOrEmpty(spawnName)) continue;

                var bundlePath = !string.IsNullOrEmpty(spawn.BundlePath) ? spawn.BundlePath : pb.Mod.Id;
                var prefabName = spawn.PrefabName?.Trim();

                if (string.IsNullOrEmpty(prefabName))
                {
                    var candidate = pb.GetAllAssetNames()
                        .FirstOrDefault(n => n.IndexOf(spawnName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!string.IsNullOrEmpty(candidate))
                        prefabName = candidate;
                }

                if (!string.IsNullOrEmpty(prefabName))
                {
                    PatchedContent.SpawnableRegistry.Registry[spawnName] = new PatchedContent.SpawnableEntry(bundlePath, prefabName);
                    Debug.Log($"[SpawnableRegistrar] Registered ExtendedSpawnable '{spawnName}' => {bundlePath} :: {prefabName}");
                }
            }
        }
    }

    public static void RegisterPlaceholdersFromPrefab(PeakBundle pb, ExtendedMod emod, GameObject biomePrefab, string? assetPathHint = null)
    {
        if (pb == null || biomePrefab == null) return;

        var assets = pb.GetAllAssetNames() ?? Array.Empty<string>();

        string PickPrefabAssetPathForGameObject(GameObject go)
        {
            if (!string.IsNullOrEmpty(assetPathHint) && assets.Any(a => a.IndexOf(assetPathHint, StringComparison.OrdinalIgnoreCase) >= 0))
                return assets.First(a => a.IndexOf(assetPathHint, StringComparison.OrdinalIgnoreCase) >= 0);

            string candidate = assets.FirstOrDefault(a => a.IndexOf(go.name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(candidate)) return candidate;

            candidate = assets.FirstOrDefault(a => a.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));
            return candidate;
        }

        var placeholders = biomePrefab.GetComponentsInChildren<Placeholder>(true);
        foreach (var ph in placeholders)
        {
            if (!string.IsNullOrEmpty(ph.SpawnableName))
            {
                var spawnName = ph.SpawnableName.Trim();
                var assetPath = PickPrefabAssetPathForGameObject(ph.gameObject) ?? string.Empty;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var bundlePath = pb.Mod.Id;
                    PatchedContent.SpawnableRegistry.Registry[spawnName] = new PatchedContent.SpawnableEntry(bundlePath, assetPath);
                    Debug.Log($"[SpawnableRegistrar] Registered Placeholder spawn '{spawnName}' => {bundlePath} :: {assetPath} (role='{ph.Role}')");
                }
            }
        }

        var luggages = biomePrefab.GetComponentsInChildren<LuggageMarker>(true);
        foreach (var lm in luggages)
        {
            if (!string.IsNullOrEmpty(lm.LuggageKey))
            {
                var key = lm.LuggageKey.Trim();
                var assetPath = PickPrefabAssetPathForGameObject(lm.gameObject) ?? string.Empty;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    PatchedContent.SpawnableRegistry.Registry[key] = new PatchedContent.SpawnableEntry(pb.Mod.Id, assetPath);
                    Debug.Log($"[SpawnableRegistrar] Registered Luggage '{key}' => {pb.Mod.Id} :: {assetPath}");
                }
            }
        }
    }
}
