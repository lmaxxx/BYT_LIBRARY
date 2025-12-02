using byt_library.Domain.Exceptions;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Subscription
{
    public string SubscriptionCode { get; private set; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    
    private Student? _student;

    private static List<Subscription> _allSubscriptions = new();
    private static readonly object _lockSubscription = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Subscription()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Subscription>();
            lock (_lockSubscription)
            {
                _allSubscriptions = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allSubscriptions = new List<Subscription>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Subscription).Name}: {ex.Message}");
            _allSubscriptions = new List<Subscription>();
        }
    }

    public Subscription() { }

    public Subscription(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new InvalidDateRangeException("End date must be after start date.");
        
        SubscriptionCode = $"SUB-{Guid.NewGuid()}";
        StartDate = startDate;
        EndDate = endDate;
        AddSubscription(this);
    }

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

    private static void AddSubscription(Subscription subscription)
    {
        if (subscription == null)
            throw new SubscriptionIsNullException(nameof(subscription), "Cannot add null subscription to extent");

        lock (_lockSubscription)
        {
            if (string.IsNullOrWhiteSpace(subscription.SubscriptionCode))
                throw new SubscriptionIsEmptyException("SubscriptionCode cannot be empty");

            if (_allSubscriptions.Any(s => s.SubscriptionCode.Equals(subscription.SubscriptionCode, StringComparison.OrdinalIgnoreCase)))
                throw new SubscriptionAlreadyExistsException($"Subscription with code {subscription.SubscriptionCode} already exists in extent");

            _allSubscriptions.Add(subscription);

            try
            {
                _persistenceService.Save(_allSubscriptions);
            }
            catch (Exception ex)
            {
                _allSubscriptions.Remove(subscription);
                throw new InvalidOperationException("Failed to persist Subscription to file", ex);
            }
        }
    }

    public static bool RemoveSubscription(string subscriptionCode)
    {
        lock (_lockSubscription)
        {
            var subscription = _allSubscriptions.FirstOrDefault(s => s.SubscriptionCode.Equals(subscriptionCode, StringComparison.OrdinalIgnoreCase));
            if (subscription != null)
            {
                return _allSubscriptions.Remove(subscription);
            }
            return false;
        }
    }

    public static Subscription? GetSubscriptionByCode(string subscriptionCode)
    {
        lock (_lockSubscription)
        {
            return _allSubscriptions.FirstOrDefault(s => s.SubscriptionCode.Equals(subscriptionCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Subscription> GetAllSubscriptions()
    {
        lock (_lockSubscription)
        {
            return _allSubscriptions.AsReadOnly();
        }
    }

    public static void ClearSubscriptionExtent()
    {
        lock (_lockSubscription)
        {
            _allSubscriptions.Clear();
            _persistenceService.Save(_allSubscriptions);
        }
    }
    
    public Student? GetStudent()
    {
        return _student;
    }

    public void SetStudent(Student student)
    {
        if (student == null)
            throw new StudentIsNullException(nameof(student), "Student must exist.");

        // avoid infinite recursion
        if (_student == student)
            return;

        // prevent assigning subscription to a different student unless first removed
        if (_student != null && _student != student)
            throw new SubscriptionAlreadyBelongsException("Subscription already assigned to another student.");

        _student = student;

        // reverse connection
        if (!student.GetSubscriptions().Contains(this))
        {
            student.AddSubscription(this);
        }
    }

    public void RemoveStudent()
    {
        if (_student == null)
            return;

        var oldStudent = _student;
        _student = null;

        // reverse connection
        if (oldStudent.GetSubscriptions().Contains(this))
        {
            oldStudent.RemoveSubscription(this);
        }
    }
}