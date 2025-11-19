public class PersonIsNullException : ArgumentNullException
{
    public PersonIsNullException(string? name, string message) : base(name, message)
    {

    }
}