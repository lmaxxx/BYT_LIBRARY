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

    private Subscription? _subscription;  
    private BorrowRecord? _borrowRecord;

    private static List<Payment> _allPayments = new();
    private static readonly object _lockPayment = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");
    
    public Subscription? GetSubscription() => _subscription;
    public BorrowRecord? GetBorrowRecord() => _borrowRecord;

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
        if (subscription != null && borrowRecord != null)
            throw new PaymentXorViolationException(
                "Payment must be attached to exactly one of Subscription or BorrowRecord.");

        if (amount <= 0)
            throw new InvalidAmountException();

        PaymentCode = $"PAY-{Guid.NewGuid()}";
        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;

        AddPayment(this);

        if (subscription != null)
            AddSubscription(subscription);
                
        if (borrowRecord != null)
            AddBorrowRecord(borrowRecord);
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
    
    public void AddSubscription(Subscription subscription)
    {
        if (subscription == null)
            throw new SubscriptionIsNullException(nameof(subscription), "Subscription cannot be null.");

        if (_borrowRecord != null)
            throw new PaymentXorViolationException(
                "Payment is already attached to a BorrowRecord; cannot attach a Subscription.");

        if (_subscription == subscription)
            return;

        _subscription = subscription;

        if (!subscription.GetPayments().Contains(this))
            subscription.AddPayment(this);
    }
    
    public void RemoveSubscription()
    {
        if (_subscription == null)
            throw new SubscriptionIsNullException(nameof(_subscription), "Payment has no subscription to remove.");

        var oldSub = _subscription;
        _subscription = null;

        if (oldSub.GetPayments().Contains(this))
            oldSub.RemovePayment(this);
    }
    
    public void AddBorrowRecord(BorrowRecord borrowRecord)
    {
        if (borrowRecord == null)
            throw new BorrowRecordIsNullException(nameof(borrowRecord), "Borrow record cannot be null.");

        if (_subscription != null)
            throw new PaymentXorViolationException(
                "Payment is already attached to a Subscription; cannot attach a BorrowRecord.");

        if (_borrowRecord == borrowRecord)
            return;

        if (borrowRecord.GetPayment() != null)
            throw new PaymentAlreadyAssignedException("BorrowRecord already has a Payment assigned.");

        _borrowRecord = borrowRecord;

        if (borrowRecord.GetPayment() != this)
            borrowRecord.AddPayment(this);
    }
    
    public void RemoveBorrowRecord()
    {
        if (_borrowRecord == null)
            throw new BorrowRecordIsNullException(nameof(_borrowRecord), "Payment has no BorrowRecord to remove.");

        var oldBr = _borrowRecord;
        _borrowRecord = null;

        if (oldBr.GetPayment() == this)
            oldBr.RemovePayment();
    }
}