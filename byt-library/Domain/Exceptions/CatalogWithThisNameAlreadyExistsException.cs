public class CatalogWithThisNameAlreadyExistsException : InvalidOperationException
{
    public CatalogWithThisNameAlreadyExistsException(string message) : base(message)
    {

    }
}