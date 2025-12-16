namespace byt_library.Domain.Exceptions;

public class AuthorAlreadyExistsException : InvalidOperationException
{
    public AuthorAlreadyExistsException(string message) : base(message)
    {
        
    }
}