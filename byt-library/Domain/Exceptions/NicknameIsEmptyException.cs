namespace byt_library.Domain.Exceptions
{
    public class NicknameIsEmptyException : Exception
    {
        public NicknameIsEmptyException() : base("Nickname cannot be empty if provided.") { }
    }
}
