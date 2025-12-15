using PEAKLib.Core;
using System.Collections;
using UnityEngine;

namespace PEAKLib.Items;

/// <summary>
/// Interface for mod items.
/// </summary>
public interface IItemContent : IContent, IGameObjectContent
{
    /// <summary>
    /// The <see cref="global::Item"/> type from Vanilla.
    /// </summary>
    public Item Item { get; }
}
