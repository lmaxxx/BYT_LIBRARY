namespace byt_library.Domain.Exceptions;

public class SubscriptionAlreadyBelongsException : InvalidOperationException
{
    public SubscriptionAlreadyBelongsException(string message) : base(message)
    {

    }
}