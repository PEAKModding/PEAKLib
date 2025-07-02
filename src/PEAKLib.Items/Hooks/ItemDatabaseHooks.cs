using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MonoDetour;
using MonoDetour.HookGen;
using On.ItemDatabase;
using UnityEngine;
using BepInEx.Logging;

namespace PEAKLib.Items.Hooks;

[MonoDetourTargets(typeof(ItemDatabase))]
static class ItemDatabaseHooks
{
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("PEAKLib.Items.Hooks.ItemDatabaseHooks");

    [MonoDetourHookInitialize]
    static void Init()
    {
        OnLoaded.Prefix(Prefix_OnLoaded);
    }

    static bool Resolve_Collision(ItemDatabase self, ref ushort id)
    {
        bool resolved = false;
        ushort origID = id;
        for(ushort i = 1; i <= 65535; i += 1)
        {
            ushort tempID = (ushort)(origID+i); // intentional overload
            if(!self.itemLookup.ContainsKey(tempID))
            {
                resolved = true;
                id = tempID;
                break;
            }
        }

        if( !resolved ) id = origID;
        return resolved;
    }

    static void Prefix_OnLoaded(ItemDatabase self)
    {
        Shader peakShader = Shader.Find("W/Peak_Standard");
        foreach (var registeredItem in ItemContent.s_RegisteredItems)
        {
            var item = registeredItem.Content.Item;

            var hash = MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(registeredItem.Mod.Id + item.name));

            ushort id = BitConverter.ToUInt16(hash, 0);
            item.itemID = id;

            if(self.itemLookup.ContainsKey(id))
            {
                // Log Collision
                Log.LogError(
                    $"ItemDatabaseHooks: Prefix_OnLoaded: Collision on hash itemID \"{id}\" for {registeredItem.Mod.Id}.{item.name}"
                );

                if (!Resolve_Collision(self,ref id))
                {
                    // Log Unresolvable Collision
                    Log.LogError(
                        $"ItemDatabaseHooks: Prefix_OnLoaded: Could not resolve collision on itemID \"{id}\" for {registeredItem.Mod.Id}.{item.name}"
                    );
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
            self.itemLookup.Add(id, item);
        }
    }
}
