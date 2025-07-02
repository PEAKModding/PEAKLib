using System.Linq;
using MonoDetour;
using MonoDetour.HookGen;
using On.ItemDatabase;
using UnityEngine;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(ItemDatabase))]
static class ItemDatabaseHooks
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        OnLoaded.Prefix(Prefix_OnLoaded);
    }

    static bool TryResolveCollision(ItemDatabase self, ref ushort id)
    {
        for (ushort i = (ushort)(id + 1); i <= ushort.MaxValue; i++)
        {
            if (!self.itemLookup.ContainsKey(i))
            {
                id = i;
                return true;
            }
        }

        return false;
    }

    static void Prefix_OnLoaded(ItemDatabase self)
    {
        Shader peakShader = Shader.Find("W/Peak_Standard");
        foreach (var registeredItem in ItemContent.s_RegisteredItems)
        {
            var item = registeredItem.Content.Item;
            item.itemID = (ushort)registeredItem.GetHashCode();

            if (self.itemLookup.ContainsKey(item.itemID))
            {
                var itemName = $"{registeredItem.Mod.Id}:{item.name}";
                ItemsPlugin.Log.LogWarning(
                    $"{nameof(Prefix_OnLoaded)}: Collision on hash itemID '{item.itemID}' for '{itemName}'"
                );

                if (TryResolveCollision(self, ref item.itemID))
                {
                    ItemsPlugin.Log.LogWarning(
                        $"{nameof(Prefix_OnLoaded)}: itemID changed to '{item.itemID}' for '{itemName}'"
                    );
                }
                else
                {
                    ItemsPlugin.Log.LogError(
                        $"{nameof(Prefix_OnLoaded)}: Could not resolve collision on itemID '{item.itemID}' for '{itemName}'"
                    );
                    continue;
                }
            }

            foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
            {
                if (renderer.material.shader.name is not "W/Peak_Standard")
                {
                    continue;
                }

                // Replace dummy shader
                renderer.material.shader = peakShader;
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
    }
}
