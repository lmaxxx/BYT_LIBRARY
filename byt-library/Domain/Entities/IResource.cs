namespace byt_library.Domain.Entities;

public interface IResource
{
    static int MaxBorrowingPeriodDays = 360;
    public string Title { get; set; }
    public string Description { get; set; }
}