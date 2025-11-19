public class AuthorAlreadyExiststException : InvalidOperationException
{
    public AuthorAlreadyExiststException(string message) : base(message)
    {
    }
}