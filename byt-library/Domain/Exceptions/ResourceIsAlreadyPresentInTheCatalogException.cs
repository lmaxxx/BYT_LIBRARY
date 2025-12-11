namespace byt_library.Domain.Exceptions;

public class ResourceIsAlreadyPresentInTheCatalogException : ArgumentException
{
    public ResourceIsAlreadyPresentInTheCatalogException(string message) : base(message)
    {
        
    }
}