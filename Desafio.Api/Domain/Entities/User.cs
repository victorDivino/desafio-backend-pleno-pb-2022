namespace Desafio.Api.Domain.Entities;

public class User
{
    public virtual int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModificationDate { get; private set; }

    protected User() { }

    private User(string name, string email)
    {
        Name = name;
        Email = email;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static User Create(string name, string email)
        => new(name, email);

    public User Update(string name, string email)
    {
        Name = name;
        Email = email;
        LastModificationDate = DateTimeOffset.UtcNow;
        return this;
    }
}