using byt_library.Domain.Enums;
using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Payment
{
    public int Id { get; private set; }
    public double Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }

    public Subscription? Subscription { get; init; }
    public BorrowRecord? BorrowRecord { get; init; }

    private static List<Payment> _allPayments = new();
    private static int _nextId = 1;
    private static readonly object _lockPayment = new();

    public Payment() { }
    
    // XOR constructor
    public Payment(
        double amount,
        DateTime paymentDate,
        PaymentMethod paymentMethod,
        Subscription? subscription = null,
        BorrowRecord? borrowRecord = null)
    {
        // enforce XOR rule
        if ((subscription == null && borrowRecord == null) ||
            (subscription != null && borrowRecord != null))
        {
            throw new ArgumentException("Payment must be attached to exactly one of Subscription or BorrowRecord.");
        }

        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
        Subscription = subscription;
        BorrowRecord = borrowRecord;
    }

    public static void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment), "Cannot add null payment to extent");

        lock (_lockPayment)
        {
            if (payment.Id == 0)
            {
                payment.Id = _nextId++;
            }

            if (_allPayments.Any(p => p.Id == payment.Id))
                throw new InvalidOperationException($"Payment with ID {payment.Id} already exists in extent");

            _allPayments.Add(payment);
        }
    }

    public static bool RemovePayment(int id)
    {
        lock (_lockPayment)
        {
            var payment = _allPayments.FirstOrDefault(p => p.Id == id);
            if (payment != null)
            {
                return _allPayments.Remove(payment);
            }
            return false;
        }
    }

    public static Payment? GetPaymentById(int id)
    {
        lock (_lockPayment)
        {
            return _allPayments.FirstOrDefault(p => p.Id == id);
        }
    }

    public static IReadOnlyList<Payment> GetAllPayments()
    {
        lock (_lockPayment)
        {
            return _allPayments.AsReadOnly();
        }
    }

    public static int GetPaymentCount()
    {
        lock (_lockPayment)
        {
            return _allPayments.Count;
        }
    }

    public static void ClearPaymentExtent()
    {
        lock (_lockPayment)
        {
            _allPayments.Clear();
            _nextId = 1;
        }
    }

    public static void SavePaymentsToFile(string filePath)
    {
        lock (_lockPayment)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allPayments, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadPaymentsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockPayment)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var paymentList = JsonSerializer.Deserialize<List<Payment>>(json, options);

            if (paymentList != null)
            {
                ClearPaymentExtent();

                foreach (var payment in paymentList)
                {
                    AddPayment(payment);
                }
            }
        }
    }
}