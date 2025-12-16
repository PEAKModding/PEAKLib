using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// A GameObject-focused interface for a registrable mod content requiring access to GameObjects to perform such operations within core lib.
/// </summary>
public interface IGameObjectContent : IContent
{
    /// <summary>
    /// Enumerates across all relevant GameObjects in the content.
    /// </summary>
    IEnumerable<GameObject> EnumerateGameObjects();
}
