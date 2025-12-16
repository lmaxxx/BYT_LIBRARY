namespace byt_library.Domain.Interfaces;

public interface IStudent
{
    DateTime EnrollmentDate { get; set; }
    bool IsAllowedToBorrow();
}
