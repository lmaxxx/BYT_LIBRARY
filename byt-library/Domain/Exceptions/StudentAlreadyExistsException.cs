public class StudentAlreadyExistsException : InvalidOperationException
{
    public StudentAlreadyExistsException(string message) : base(message)
    {

    }
}