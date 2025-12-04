namespace byt_library.Domain.Exceptions;

public class TranslationOwnerIsNullException : ArgumentNullException
{
    public TranslationOwnerIsNullException(string message) : base(message) { }

    public TranslationOwnerIsNullException(string paramName, string message)
        : base(paramName, message) { }
}
