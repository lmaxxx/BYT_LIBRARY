public class AuthorWithSuchNameAlreadyExistsException : InvalidOperationException
{
    public AuthorWithSuchNameAlreadyExistsException(string message) : base(message)
    {

    }
}