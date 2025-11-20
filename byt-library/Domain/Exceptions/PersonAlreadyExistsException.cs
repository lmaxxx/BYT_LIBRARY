public class PersonAlreadyExistsException : InvalidOperationException
{
    public PersonAlreadyExistsException(string message) : base(message)
    {

    }
}