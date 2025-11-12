using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Subscription
{
    public int Id { get; private set; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    private static List<Subscription> _allSubscriptions = new();
    private static int _nextId = 1;
    private static readonly object _lockSubscription = new();

    public Subscription() { }

    public Subscription(DateTime startDate, DateTime endDate)
    {
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
            throw new ArgumentNullException(nameof(subscription), "Cannot add null subscription to extent");

        lock (_lockSubscription)
        {
            if (subscription.Id == 0)
            {
                subscription.Id = _nextId++;
            }

            if (_allSubscriptions.Any(s => s.Id == subscription.Id))
                throw new InvalidOperationException($"Subscription with ID {subscription.Id} already exists in extent");

            _allSubscriptions.Add(subscription);
        }
    }

    public static bool RemoveSubscription(int id)
    {
        lock (_lockSubscription)
        {
            var subscription = _allSubscriptions.FirstOrDefault(s => s.Id == id);
            if (subscription != null)
            {
                return _allSubscriptions.Remove(subscription);
            }
            return false;
        }
    }

    public static Subscription? GetSubscriptionById(int id)
    {
        lock (_lockSubscription)
        {
            return _allSubscriptions.FirstOrDefault(s => s.Id == id);
        }
    }

    public static IReadOnlyList<Subscription> GetAllSubscriptions()
    {
        lock (_lockSubscription)
        {
            return _allSubscriptions.AsReadOnly();
        }
    }

    public static int GetSubscriptionCount()
    {
        lock (_lockSubscription)
        {
            return _allSubscriptions.Count;
        }
    }

    public static void ClearSubscriptionExtent()
    {
        lock (_lockSubscription)
        {
            _allSubscriptions.Clear();
            _nextId = 1;
        }
    }

    public static void SaveSubscriptionsToFile(string filePath)
    {
        lock (_lockSubscription)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allSubscriptions, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadSubscriptionsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockSubscription)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var subscriptionList = JsonSerializer.Deserialize<List<Subscription>>(json, options);

            if (subscriptionList != null)
            {
                ClearSubscriptionExtent();

                foreach (var subscription in subscriptionList)
                {
                    AddSubscription(subscription);
                }
            }
        }
    }
}