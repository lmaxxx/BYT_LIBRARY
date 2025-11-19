public class PaymentIsNullException : ArgumentNullException
{
    public PaymentIsNullException(string? name, string message) : base(name, message)
    {

    }
}