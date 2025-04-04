using Example.SharedKernel.Models;

namespace Example.Api.Features.Orders;

public sealed class AddressId : EntityId<Guid>
{
    private AddressId(Guid value) : base(value)
    {
    }

    public static AddressId CreateUnique()
    {
        return new AddressId(Guid.NewGuid());
    }

    public static AddressId Create(Guid value)
    {
        return new AddressId(value);
    }
}


public sealed class Address : Entity<AddressId>
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    public Address(AddressId id, string street, string city, string state, string zipCode) : base(id)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string city, string state, string zipCode, AddressId? addressId = null)
    {
        AddressId id;
        if (addressId is null)
        {
            id = AddressId.CreateUnique();
        }
        else
        {
            id = addressId;
        }

        return new Address(id, street, city, state, zipCode);
    }
}