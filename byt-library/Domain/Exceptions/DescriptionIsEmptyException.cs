namespace byt_library.Domain.Exceptions
{
    public class DescriptionIsEmptyException : Exception
    {
        public DescriptionIsEmptyException() : base("Description cannot be empty.") { }
    }
}
