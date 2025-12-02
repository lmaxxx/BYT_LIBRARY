namespace byt_library.Domain.Exceptions
{
    public class InvalidBorrowDaysException : Exception
    {
        public InvalidBorrowDaysException() : base("Borrow days must be a positive number.") { }
    }
}
