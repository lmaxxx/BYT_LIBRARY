public class AuthorIsNullException : ArgumentNullException
{
    public AuthorIsNullException(string? name, string message) : base(name, message)
    {

    }
}