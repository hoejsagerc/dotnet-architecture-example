namespace Example.SharedKernel.Models;

public abstract class EntityId<TId> : ValueObject
{
    public TId Value { get; }

    public static explicit operator TId(EntityId<TId> id)
            => id.Value;

    protected EntityId(TId value)
    {
        Value = value;
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string? ToString() => Value?.ToString() ?? base.ToString();

#pragma warning disable CS8618
    protected EntityId()
    {
    }
#pragma warning restore CS8618
}

