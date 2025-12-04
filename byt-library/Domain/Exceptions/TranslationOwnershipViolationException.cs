namespace byt_library.Domain.Exceptions;

public class TranslationOwnershipViolationException : InvalidOperationException
{
    public TranslationOwnershipViolationException(string message) : base(message) { }

    public TranslationOwnershipViolationException(string message, Exception innerException)
        : base(message, innerException) { }
}
