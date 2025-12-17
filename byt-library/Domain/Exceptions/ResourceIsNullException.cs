namespace byt_library.Domain.Exceptions;

public class ResourceIsNullException : ArgumentNullException
{
    public ResourceIsNullException(string? name, string message) : base(name, message)
    {

    }
}