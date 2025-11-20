public class AuthorWithSuchNicknameAlreadyExistsException : InvalidOperationException
{
    public AuthorWithSuchNicknameAlreadyExistsException(string message) : base(message) 
    {
    
    }
}