using byt_library.Domain.Enums;

namespace byt_library.Domain.Entities;

public class BorrowRecord
{
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowRecordStatus Status { get; set; }
    public double FineAmount { get; set; }
    public string BorrowCode { get; set; }

    public double CalculateFine()
    {
        if (ReturnDate == null || ReturnDate <= DueDate)
            return 0;

        var lateDays = (ReturnDate.Value - DueDate).Days;
        FineAmount = lateDays * 1.0; // $1 per day
        return FineAmount;
    }
    
    public void CancelBorrowRecordRequest()
    {
        if (Status == BorrowRecordStatus.Ongoing)
            throw new InvalidOperationException("Cannot cancel an active borrow record.");

        Status = BorrowRecordStatus.Canceled;
    }

    // sets borrow and due dates, marks as ongoing
    public void ActivateBorrowRecord(int borrowDays = 30)
    {
        BorrowDate = DateTime.Now;
        DueDate = BorrowDate.AddDays(borrowDays);
        Status = BorrowRecordStatus.Ongoing;
    }

    // checks if overdue and not yet returned
    public bool IsBorrowRecordDelayed()
    {
        return Status == BorrowRecordStatus.Ongoing && DateTime.Now > DueDate;
    }

    //  sets return date, update status, calculate fine
    public void ReturnBorrowRecord()
    {
        if (Status != BorrowRecordStatus.Ongoing)
            throw new InvalidOperationException("Borrow record is not active.");

        ReturnDate = DateTime.Now;
        Status = BorrowRecordStatus.Returned;
        CalculateFine();
    }

    // just a simple random code
    public void GenerateBorrowCode()
    {
        BorrowCode = $"BR-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}