using Example.SharedKernel.Models;

namespace Example.Api.Features.Orders;

public sealed class CustomerId : EntityId<Guid>
{
    private CustomerId(Guid value) : base(value)
    {
    }

    public static CustomerId CreateUnique()
    {
        return new CustomerId(Guid.NewGuid());
    }

    public static CustomerId Create(Guid value)
    {
        return new CustomerId(value);
    }
}


public sealed class Customer : Entity<CustomerId>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }


    public Customer(CustomerId id, string firstName, string lastName, string email) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }


    public static Customer Create(string firstName, string lastName, string email, CustomerId? customerId = null)
    {
        CustomerId id;
        if (customerId is null)
        {
            id = CustomerId.CreateUnique();
        }
        else
        {
            id = customerId;
        }

        return new Customer(id, firstName, lastName, email);
    }
}