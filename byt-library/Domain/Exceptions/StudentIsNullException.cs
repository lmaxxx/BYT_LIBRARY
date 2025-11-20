public class StudentIsNullException : ArgumentNullException
{
    public StudentIsNullException(string? name, string message) : base(name, message)
    {

    }
}