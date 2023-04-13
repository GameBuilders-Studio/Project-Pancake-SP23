public interface ICombinable
{
    public bool TryCombineWith(InteractionBehaviour other);
}

public interface IUsable
{
    public void OnUseStart();
    public void OnUseEnd();
    public bool Enabled { get; }
}

public interface IUsableWhileCarried 
{
    public void OnUseStart();
    public void OnUseEnd();
    public bool Enabled {get;}
}

public interface IHasCarryable
{
    public Carryable PopCarryable();
}