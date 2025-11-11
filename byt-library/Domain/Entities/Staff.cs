namespace byt_library.Domain.Entities;

public class Staff : Person
{
    public string Department { get; set; }

    public Staff(string firstName, string lastName, DateTime dateOfBirth, string department, string? email = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        Department = department;
    }
}