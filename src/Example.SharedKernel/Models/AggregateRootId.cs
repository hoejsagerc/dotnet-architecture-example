namespace Example.SharedKernel.Models;

public abstract class AggregateRootId<TId> : EntityId<TId>
{
    protected AggregateRootId(TId value) : base(value)
    {

    }
}
