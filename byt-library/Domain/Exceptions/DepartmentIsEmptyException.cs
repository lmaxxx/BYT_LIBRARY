namespace byt_library.Domain.Exceptions
{
    public class DepartmentIsEmptyException : Exception
    {
        public DepartmentIsEmptyException() : base("Department cannot be empty.") { }
    }
}
