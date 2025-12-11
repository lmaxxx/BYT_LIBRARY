namespace byt_library.Domain.Exceptions;

public class ResourceIsNotPresentInTheCatalogException : ArgumentException
{
    public ResourceIsNotPresentInTheCatalogException(string message) : base(message)
    {
        
    }
}