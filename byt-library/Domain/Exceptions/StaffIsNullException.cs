public class StaffIsNullException : ArgumentNullException
{
    public StaffIsNullException(string? name, string message) : base(name, message)
    {

    }
}