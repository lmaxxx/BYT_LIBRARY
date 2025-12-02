namespace byt_library.Domain.Exceptions
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException() : base("Payment amount must be positive.") { }
    }
}
