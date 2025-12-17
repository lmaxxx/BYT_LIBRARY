namespace byt_library.Domain.Exceptions;

public class ResourceAlreadyHaveChildClassException : InvalidOperationException
{
    public ResourceAlreadyHaveChildClassException(string message) : base(message)
    {

    }
}