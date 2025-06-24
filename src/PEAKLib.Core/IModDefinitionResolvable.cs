namespace PEAKLib.Core;

/// <summary>
/// An interface for a returning a ModDefinition.
/// </summary>
public interface IModDefinitionResolvable
{
    /// <summary>
    /// Returns the <see cref="ModDefinition"/> representation of this mod
    /// if this is not one already.
    /// </summary>
    /// <returns>The <see cref="ModDefinition"/> representation.</returns>
    public ModDefinition Resolve();
}
