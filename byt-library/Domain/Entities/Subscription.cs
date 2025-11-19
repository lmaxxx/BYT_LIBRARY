namespace byt_library.Domain.Entities;

public class Subscription
{
    public string SubscriptionCode { get; private set; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    private static List<Subscription> _allSubscriptions = new();
    private static readonly object _lockSubscription = new();

    public Subscription() { }

    public Subscription(DateTime startDate, DateTime endDate)
    {
        SubscriptionCode = $"SUB-{Guid.NewGuid()}";
        StartDate = startDate;
        EndDate = endDate;
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

    public static void AddSubscription(Subscription subscription)
    {
        if (subscription == null)
            throw new SubscriptionIsNullException(nameof(subscription), "Cannot add null subscription to extent");

        lock (_lockSubscription)
        {
            if (string.IsNullOrWhiteSpace(subscription.SubscriptionCode))
                throw new ArgumentException("SubscriptionCode cannot be empty");

            if (_allSubscriptions.Any(s => s.Id == subscription.Id))
                throw new SubscriptionAlreadyExistsException($"Subscription with ID {subscription.Id} already exists in extent");

            _allSubscriptions.Add(subscription);
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
        }
    }
}