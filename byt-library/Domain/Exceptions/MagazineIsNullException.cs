public class MagazineIsNullException : ArgumentNullException
{
    public MagazineIsNullException(string? name, string message) : base(name, message)
    {

    }
}