namespace byt_library.Domain.Entities;

public class Student : Person
{
    public DateTime EnrollmentDate { get; set; }

    public Student(string firstName, string lastName, DateTime dateOfBirth, DateTime enrollmentDate, string? email = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        EnrollmentDate = enrollmentDate;
    }

    public bool isAllowedToBorrow()
    {
        return false;
    }
}