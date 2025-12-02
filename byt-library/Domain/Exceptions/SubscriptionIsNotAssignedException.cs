namespace byt_library.Domain.Exceptions;

public class SubscriptionIsNotAssignedException : InvalidOperationException
{
    public SubscriptionIsNotAssignedException(string message) : base(message)
    {
        
    }
}