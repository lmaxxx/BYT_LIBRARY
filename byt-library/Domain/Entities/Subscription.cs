namespace byt_library.Domain.Entities;

public class Subscription
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public bool IsActive()
    {
        return DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
    
    // 25 $ per month
    public double CalculateCost()
    {
        // calculate number of months 
        double totalDays = (EndDate - StartDate).TotalDays;
        double months = Math.Ceiling(totalDays / 30); // round up partial months

        return months * 25.0;
    }
}