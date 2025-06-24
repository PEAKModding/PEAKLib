using PEAKLib.Core;

namespace PEAKLib.Items;

/// <summary>
/// Interface for mod items.
/// </summary>
public interface IModItem : IModContent
{
    /// <summary>
    /// The <see cref="global::Item"/> type from Vanilla.
    /// </summary>
    public Item Item { get; }
}
