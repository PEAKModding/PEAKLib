using UnityEngine;

namespace PEAKLib.Core;

/// <summary>
/// A component-focused interface for a registrable mod content allowing component-level operations within core lib.
/// </summary>
public interface IComponentContent
{
    /// <summary>
    /// Link to primary component associated with content. 
    /// </summary>
    Component Component { get; }
}
