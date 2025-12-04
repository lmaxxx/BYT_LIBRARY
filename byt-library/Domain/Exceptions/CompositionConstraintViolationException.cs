namespace byt_library.Domain.Exceptions;

public class CompositionConstraintViolationException : InvalidOperationException
{
    public CompositionConstraintViolationException(string message) : base(message) { }

    public CompositionConstraintViolationException(string message, Exception innerException)
        : base(message, innerException) { }
}
