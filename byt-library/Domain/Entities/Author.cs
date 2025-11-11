namespace byt_library.Domain.Entities;

public class Author : Person
{
    public string? Nickname { get; set; }

    public Author(string firstName, string lastName, DateTime dateOfBirth, string? email = null, string? nickname = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        Nickname = nickname;
    }
}