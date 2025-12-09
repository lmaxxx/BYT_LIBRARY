using byt_library.Domain.Exceptions;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Subscription
{
    public string SubscriptionCode { get; private set; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    
    private Student _student;
    private readonly HashSet<Payment> _payments = new();

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

    public Subscription(DateTime startDate, DateTime endDate, Student student, ICollection<Payment> payments)
    {
        if (endDate <= startDate)
            throw new InvalidDateRangeException("End date must be after start date.");
        
        if (student == null)
            throw new StudentIsNullException(nameof(student), "Subscription must have a student");
        
        if (payments == null || !payments.Any())
            throw new PaymentIsNullException(nameof(payments), "Subscription must have at least one payment");
        
        SubscriptionCode = $"SUB-{Guid.NewGuid()}";
        StartDate = startDate;
        EndDate = endDate;
        
        AddSubscription(this);  // create subscription
        SetStudent(student);  // assign student
        
        foreach (var payment in payments)
            AddPayment(payment);    // assign each payment
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
        if (student.GetSubscription() != this)
            student.AddSubscription(this);
    }
    
    public IReadOnlyCollection<Payment> GetPayments()
        => _payments.ToList().AsReadOnly();
    
    public void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new PaymentIsNullException(nameof(payment), "Payment cannot be null.");

        if (_payments.Contains(payment))
            return;

        if (payment.GetBorrowRecord() != null)
            throw new PaymentXorViolationException("Payment already belongs to a BorrowRecord.");

        _payments.Add(payment);

        if (payment.GetSubscription() != this)
            payment.AddSubscription(this);
    }
    
    public void RemovePayment(Payment payment)
    {
        if (payment == null)
            throw new PaymentIsNullException(nameof(payment), "Payment cannot be null.");

        if (!_payments.Contains(payment))
            return;

        _payments.Remove(payment);

        if (payment.GetSubscription() == this)
            payment.RemoveSubscription();
    }
}