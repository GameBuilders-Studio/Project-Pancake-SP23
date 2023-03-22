public interface IInteractionInterface {}

public interface ICombinable : IInteractionInterface
{
    public bool TryCombineWith(InteractionProvider other);
}

public interface IUsable : IInteractionInterface
{
    public void OnUseStart();
    public void OnUseEnd();
    public bool Enabled {get;}
}

public interface IHasCarryable : IInteractionInterface
{
    public Carryable PopCarryable();
}