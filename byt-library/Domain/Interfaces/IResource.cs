namespace byt_library.Domain.Interfaces;

public interface IResource
{
    static int MaxBorrowingPeriodDays = 360;
    public string Title { get; set; }
    public string Description { get; set; }
}