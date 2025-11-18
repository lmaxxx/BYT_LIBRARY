public class CatalogIsNullException : ArgumentNullException
{
    public CatalogIsNullException(string? name, string message) : base(name, message)
    {

    }
}