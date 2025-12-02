namespace byt_library.Domain.Exceptions
{
    public class InvalidEnrollmentDateException : Exception
    {
        public InvalidEnrollmentDateException(string message) : base(message) { }
    }
}
