namespace byt_library.Domain.Exceptions;

public class PaymentXorViolationException : Exception
{
    public PaymentXorViolationException(string message): base(message) {}
}