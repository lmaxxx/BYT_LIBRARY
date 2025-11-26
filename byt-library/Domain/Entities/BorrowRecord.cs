using byt_library.Domain.Enums;
using byt_library.Domain.Services;

namespace byt_library.Domain.Entities;

public class BorrowRecord
{
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowRecordStatus Status { get; set; }
    public string BorrowCode { get; set; }

    public BorrowRecord()
    {
        BorrowDate = DateTime.Now;
        DueDate = BorrowDate.AddDays(30);
        Status = BorrowRecordStatus.Ongoing;
        ReturnDate = null;
        GenerateBorrowCode();
        AddBorrowRecord(this);
    }

    public BorrowRecord(int borrowDays = 30)
    {
        BorrowDate = DateTime.Now;
        DueDate = BorrowDate.AddDays(borrowDays);
        Status = BorrowRecordStatus.Ongoing;
        ReturnDate = null;
        GenerateBorrowCode();
        AddBorrowRecord(this);
    }

    public double FineAmount
    {
        get
        {
            if (ReturnDate == null || ReturnDate <= DueDate)
                return 0;

            double lateDays = (ReturnDate.Value - DueDate).Days;
            return lateDays;
        }
    }

    private static List<BorrowRecord> _allBorrowRecords = new();
    private static readonly object _lockBorrowRecord = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static BorrowRecord()
    {
        try
        {
            var loadedItems = _persistenceService.Load<BorrowRecord>();
            lock (_lockBorrowRecord)
            {
                _allBorrowRecords = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allBorrowRecords = new List<BorrowRecord>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(BorrowRecord).Name}: {ex.Message}");
            _allBorrowRecords = new List<BorrowRecord>();
        }
    }

    public double CalculateFine()
    {
        if (ReturnDate == null || ReturnDate <= DueDate)
            return 0;

        double lateDays = (ReturnDate.Value - DueDate).Days;
        return lateDays; // 1$ per day
    }
    
    public void CancelBorrowRecordRequest()
    {
        if (Status == BorrowRecordStatus.Ongoing)
            throw new BorrowRecordIsActiveException("Cannot cancel an active borrow record.");

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
            throw new BorrowRecordIsInactiveException("Borrow record is not active.");

        ReturnDate = DateTime.Now;
        Status = BorrowRecordStatus.Returned;
        CalculateFine();
    }

    // just a simple random code
    public void GenerateBorrowCode()
    {
        BorrowCode = $"BR-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private static void AddBorrowRecord(BorrowRecord borrowRecord)
    {
        if (borrowRecord == null)
            throw new BorrowRecordIsNullException(nameof(borrowRecord), "Cannot add null borrow record to extent");

        lock (_lockBorrowRecord)
        {
            if (string.IsNullOrWhiteSpace(borrowRecord.BorrowCode))
                throw new BorrowCodeIsEmptyException("BorrowCode cannot be empty");

            if (_allBorrowRecords.Any(br => br.BorrowCode.Equals(borrowRecord.BorrowCode, StringComparison.OrdinalIgnoreCase)))
                throw new BorrowRecordAlreadyExistsException($"BorrowRecord with code {borrowRecord.BorrowCode} already exists in extent");

            _allBorrowRecords.Add(borrowRecord);

            try
            {
                _persistenceService.Save(_allBorrowRecords);
            }
            catch (Exception ex)
            {
                _allBorrowRecords.Remove(borrowRecord);
                throw new InvalidOperationException("Failed to persist BorrowRecord to file", ex);
            }
        }
    }

    public static bool RemoveBorrowRecord(string borrowCode)
    {
        lock (_lockBorrowRecord)
        {
            var borrowRecord = _allBorrowRecords.FirstOrDefault(br => br.BorrowCode.Equals(borrowCode, StringComparison.OrdinalIgnoreCase));
            if (borrowRecord != null)
            {
                return _allBorrowRecords.Remove(borrowRecord);
            }
            return false;
        }
    }

    public static BorrowRecord? GetBorrowRecordByCode(string borrowCode)
    {
        lock (_lockBorrowRecord)
        {
            return _allBorrowRecords.FirstOrDefault(br => br.BorrowCode.Equals(borrowCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<BorrowRecord> GetAllBorrowRecords()
    {
        lock (_lockBorrowRecord)
        {
            return _allBorrowRecords.AsReadOnly();
        }
    }

    public static void ClearBorrowRecordExtent()
    {
        lock (_lockBorrowRecord)
        {
            _allBorrowRecords.Clear();
        }
    }
}