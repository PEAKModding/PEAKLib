using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PEAKLib.Core;
using UnityEngine;

namespace PEAKLib.Items;

/// <summary>
/// Helper library for item registration
/// </summary>
public static class ItemRegistrar
{
    internal static ItemDatabase? ItemDatabaseLoaded;

    private static Dictionary<string, Shader> _peakShaders = null!;
    internal static Dictionary<string, Shader> PeakShaders
    {
        get
        {
            if (_peakShaders == null)
            {
                var shaders = new List<string>
                {
                    "W/Peak_Standard",
                    "W/Character",
                    "W/Peak_Transparent",
                    "W/Peak_Glass",
                    "W/Peak_Clip",
                    "W/Peak_glass_liquid",
                    "W/Peak_GroundTransition",
                    "W/Peak_Guidebook",
                    "W/Peak_Honey",
                    "W/Peak_Ice",
                    "W/Peak_Rock",
                    "W/Peak_Rope",
                    "W/Peak_Splash",
                    "W/Peak_Waterfall",
                    "W/Vine",
                };
                _peakShaders = new();
                foreach (var sh in shaders)
                {
                    var shader = Shader.Find(sh);
                    if (shader == null)
                    {
                        ItemsPlugin.Log.LogWarning(
                            $"{nameof(PeakShaders)}: Shader {sh} was not found."
                        );
                        continue;
                    }
                    _peakShaders[sh] = shader;
                }
            }
            return _peakShaders;
        }
    }

    internal static void RegisterIfTooLate(RegisteredContent<ItemContent> registeredItem)
    {
        if (ItemDatabaseLoaded != null)
        {
            FinishRegisterItem(ItemDatabaseLoaded, registeredItem);
        }
    }

    internal static void FinishRegisterItem(ItemDatabase self, RegisteredContent<ItemContent> registeredItem)
    {
        var item = registeredItem.Content.Item;

        var hash = MD5.Create()
            .ComputeHash(Encoding.UTF8.GetBytes(registeredItem.Mod.Id + item.name));

        item.itemID = BitConverter.ToUInt16(hash, 0);

        if (self.itemLookup.ContainsKey(item.itemID))
        {
            var itemName = $"{registeredItem.Mod.Id}:{item.name}";
            ItemsPlugin.Log.LogWarning(
                $"{nameof(FinishRegisterItem)}: Collision on hash itemID '{item.itemID}' for '{itemName}'"
            );

            if (TryResolveCollision(self, ref item.itemID))
            {
                ItemsPlugin.Log.LogWarning(
                    $"{nameof(FinishRegisterItem)}: itemID changed to '{item.itemID}' for '{itemName}'"
                );
            }
            else
            {
                ItemsPlugin.Log.LogError(
                    $"{nameof(FinishRegisterItem)}: Could not resolve collision on itemID '{item.itemID}' for '{itemName}'"
                );
                return;
            }
        }

        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
        {
            if (!PeakShaders.ContainsKey(renderer.material.shader.name))
            {
                continue;
            }

            // Replace dummy shader
            renderer.material.shader = PeakShaders[renderer.material.shader.name];
        }

        // Fix smoke
        item
            .gameObject.GetComponentInChildren<ParticleSystem>()
            .GetComponent<ParticleSystemRenderer>()
            .material = Resources
            .FindObjectsOfTypeAll<Material>()
            .ToList()
            .Find(x => x.name == "Smoke");

        // Add item to database
        self.Objects.Add(item);
        self.itemLookup.Add(item.itemID, item);
    }

    static bool TryResolveCollision(ItemDatabase self, ref ushort id)
    {
        // This should overflow and loop back to check every entry in the dictionary.
        for (ushort i = (ushort)(id + 1); i != id; i++)
        {
            if (!self.itemLookup.ContainsKey(i))
            {
                id = i;
                return true;
            }
        }

        return false;
    }
}
