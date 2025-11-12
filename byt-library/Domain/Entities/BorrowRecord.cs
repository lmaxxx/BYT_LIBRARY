using byt_library.Domain.Enums;
using System.Text.Json;

namespace byt_library.Domain.Entities;

public class BorrowRecord
{
    public int Id { get; private set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowRecordStatus Status { get; set; }
    
    public BorrowRecord(int borrowDays = 30)
    {
        BorrowDate = DateTime.Now;
        DueDate = BorrowDate.AddDays(borrowDays);
        Status = BorrowRecordStatus.Ongoing;
        ReturnDate = null;
        GenerateBorrowCode();
    }
    
    public double FineAmount
    {
        get
        {
            if (ReturnDate == null || ReturnDate <= DueDate)
                return 0;

            double lateDays = (ReturnDate.Value - DueDate).Days;
            return lateDays; // 1$ per day
        }
    }
    public string BorrowCode { get; set; }

    private static List<BorrowRecord> _allBorrowRecords = new();
    private static int _nextId = 1;
    private static readonly object _lockBorrowRecord = new();

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

    public static void AddBorrowRecord(BorrowRecord borrowRecord)
    {
        if (borrowRecord == null)
            throw new ArgumentNullException(nameof(borrowRecord), "Cannot add null borrow record to extent");

        lock (_lockBorrowRecord)
        {
            if (borrowRecord.Id == 0)
            {
                borrowRecord.Id = _nextId++;
            }

            if (_allBorrowRecords.Any(br => br.Id == borrowRecord.Id))
                throw new InvalidOperationException($"BorrowRecord with ID {borrowRecord.Id} already exists in extent");

            _allBorrowRecords.Add(borrowRecord);
        }
    }

    public static bool RemoveBorrowRecord(int id)
    {
        lock (_lockBorrowRecord)
        {
            var borrowRecord = _allBorrowRecords.FirstOrDefault(br => br.Id == id);
            if (borrowRecord != null)
            {
                return _allBorrowRecords.Remove(borrowRecord);
            }
            return false;
        }
    }

    public static BorrowRecord? GetBorrowRecordById(int id)
    {
        lock (_lockBorrowRecord)
        {
            return _allBorrowRecords.FirstOrDefault(br => br.Id == id);
        }
    }

    public static IReadOnlyList<BorrowRecord> GetAllBorrowRecords()
    {
        lock (_lockBorrowRecord)
        {
            return _allBorrowRecords.AsReadOnly();
        }
    }

    public static int GetBorrowRecordCount()
    {
        lock (_lockBorrowRecord)
        {
            return _allBorrowRecords.Count;
        }
    }

    public static void ClearBorrowRecordExtent()
    {
        lock (_lockBorrowRecord)
        {
            _allBorrowRecords.Clear();
            _nextId = 1;
        }
    }

    public static void SaveBorrowRecordsToFile(string filePath)
    {
        lock (_lockBorrowRecord)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allBorrowRecords, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadBorrowRecordsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockBorrowRecord)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var borrowRecordList = JsonSerializer.Deserialize<List<BorrowRecord>>(json, options);

            if (borrowRecordList != null)
            {
                ClearBorrowRecordExtent();

                foreach (var borrowRecord in borrowRecordList)
                {
                    AddBorrowRecord(borrowRecord);
                }
            }
        }
    }
}