namespace byt_library.Domain.Exceptions;

public class CatalogNotFoundInResourceException : InvalidOperationException
{
    public CatalogNotFoundInResourceException(string message) : base(message)
    {
    
    }
}