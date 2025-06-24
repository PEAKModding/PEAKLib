namespace PEAKLib.Core;

public interface IModContent<T> : IModContent
    where T : IModContent<T>
{
    public RegisteredModContent<T> Register(ModDefinition owner);
}

public interface IModContent
{
    public IRegisteredModContent Register(ModDefinition owner);
}
