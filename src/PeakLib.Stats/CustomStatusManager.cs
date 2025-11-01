using System;
using System.Collections.Generic;
using System.Linq;
using PEAKLib.Core;
using UnityEngine;
using static CharacterAfflictions;

namespace PEAKLib.Stats;

/// <summary>
/// Stores and indexes custom statuses.
/// This is needed because currentStatuses is an array, so indexes need to be compacted.
/// </summary>
public static class CustomStatusManager
{
    static SortedList<string, Status> registered = new();

    /// <summary>
    /// List of registered status effects
    /// </summary>
    public static IList<Status> Statuses => registered.Values;

    /// <summary>
    /// Number of registered custom status effects
    /// </summary>
    public static int Length => registered.Count;

    internal static void RegisterStatus(Status status, ModDefinition owner)
    {
        string key = $"{owner.Id}->{status.Name}";
        if (registered.ContainsKey(key))
        {
            throw new ArgumentException(
                $"Status effect with name {key} already registered. Choose a unique name."
            );
        }
        registered.Add(key, status);
        ReIndex();
    }

    internal static Status StatusByType(STATUSTYPE type)
    {
        return registered.FirstOrDefault(x => x.Value.Type == type).Value;
    }

    private static void ReIndex()
    {
        var vanillaStatuses = Enum.GetValues(typeof(STATUSTYPE)).OfType<STATUSTYPE>().ToList();
        List<Status> reg = registered.Values.ToList();
        for (int i = 0, j = 0; i < vanillaStatuses.Count + reg.Count && j < reg.Count; i++)
        {
            // place new status effects into un-defined vanilla enum values
            if (!Enum.IsDefined(typeof(STATUSTYPE), i))
            {
                reg[j].Index = i;
                j++;
            }
        }
    }
}
