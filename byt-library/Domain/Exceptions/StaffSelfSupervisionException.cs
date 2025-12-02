namespace byt_library.Domain.Exceptions;

public class StaffSelfSupervisionException : InvalidOperationException
{
    public StaffSelfSupervisionException(string message) : base(message) { }
}