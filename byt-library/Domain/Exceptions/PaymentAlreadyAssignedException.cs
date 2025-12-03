using byt_library.Domain.Entities;

namespace byt_library.Domain.Exceptions;

public class PaymentAlreadyAssignedException : Exception
{
    public PaymentAlreadyAssignedException(string message) : base(message) {}
}