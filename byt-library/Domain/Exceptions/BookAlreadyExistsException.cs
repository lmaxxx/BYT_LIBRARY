public class BookAlreadyExistsException : InvalidOperationException
{
    public BookAlreadyExistsException(string message) : base(message)
    {

    }
}