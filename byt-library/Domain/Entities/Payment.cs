using byt_library.Domain.Enums;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Payment
{
    public string PaymentCode { get; private set; } = string.Empty;
    public double Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }

    public Subscription? Subscription { get; init; }
    public BorrowRecord? BorrowRecord { get; init; }

    private static List<Payment> _allPayments = new();
    private static readonly object _lockPayment = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Payment()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Payment>();
            lock (_lockPayment)
            {
                _allPayments = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allPayments = new List<Payment>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Payment).Name}: {ex.Message}");
            _allPayments = new List<Payment>();
        }
    }

    public Payment() { }
    
    public Payment(
        double amount,
        DateTime paymentDate,
        PaymentMethod paymentMethod,
        Subscription? subscription = null,
        BorrowRecord? borrowRecord = null)
    {
        if ((subscription == null && borrowRecord == null) ||
            (subscription != null && borrowRecord != null))
        {
            throw new PaymentIsNotAttachedException("Payment must be attached to exactly one of Subscription or BorrowRecord.");
        }
        
        if (amount <= 0)
            throw new InvalidAmountException();

        PaymentCode = $"PAY-{Guid.NewGuid()}";
        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
        Subscription = subscription;
        BorrowRecord = borrowRecord;
        AddPayment(this);
    }

    private static void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new PaymentIsNullException(nameof(payment), "Cannot add null payment to extent");

        lock (_lockPayment)
        {
            if (string.IsNullOrWhiteSpace(payment.PaymentCode))
                throw new PaymentIsEmptyException("PaymentCode cannot be empty");

            if (_allPayments.Any(p => p.PaymentCode.Equals(payment.PaymentCode, StringComparison.OrdinalIgnoreCase)))
                throw new PaymentAlreadyExistsException($"Payment with code {payment.PaymentCode} already exists in extent");

            _allPayments.Add(payment);

            try
            {
                _persistenceService.Save(_allPayments);
            }
            catch (Exception ex)
            {
                _allPayments.Remove(payment);
                throw new InvalidOperationException("Failed to persist Payment to file", ex);
            }
        }
    }

    public static bool RemovePayment(string paymentCode)
    {
        lock (_lockPayment)
        {
            var payment = _allPayments.FirstOrDefault(p => p.PaymentCode.Equals(paymentCode, StringComparison.OrdinalIgnoreCase));
            if (payment != null)
            {
                return _allPayments.Remove(payment);
            }
            return false;
        }
    }

    public static Payment? GetPaymentByCode(string paymentCode)
    {
        lock (_lockPayment)
        {
            return _allPayments.FirstOrDefault(p => p.PaymentCode.Equals(paymentCode, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Payment> GetAllPayments()
    {
        lock (_lockPayment)
        {
            return _allPayments.AsReadOnly();
        }
    }

    public static void ClearPaymentExtent()
    {
        lock (_lockPayment)
        {
            _allPayments.Clear();
            _persistenceService.Save(_allPayments);
        }
    }
}