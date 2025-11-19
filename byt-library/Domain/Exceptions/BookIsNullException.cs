public class BookIsNullException : ArgumentNullException
{
    public BookIsNullException(string? name, string message) : base(name, message)
    {
    }
}