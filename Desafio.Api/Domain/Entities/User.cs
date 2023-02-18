namespace Desafio.Api.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private User(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public static User Create(string name, string email)
        => new(name, email);

    public User Update(string name, string email)
    {
        Name = name;
        Email = email;
        return this;
    }
}