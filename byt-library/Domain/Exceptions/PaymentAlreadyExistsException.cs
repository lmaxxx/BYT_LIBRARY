public class PaymentAlreadyExistsException : InvalidOperationException
{
    public PaymentAlreadyExistsException(string message) : base(message)
    {

    }
}