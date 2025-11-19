public class PersonWithThisEmailAlreadyExistsException : InvalidOperationException
{
    public PersonWithThisEmailAlreadyExistsException(string message) : base(message)
    {

    }
}