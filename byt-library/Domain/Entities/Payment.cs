using byt_library.Domain.Enums;

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
            throw new PaymentGoalIsNotDefinedException("Payment must be attached to exactly one of Subscription or BorrowRecord.");
        }

        PaymentCode = $"PAY-{Guid.NewGuid()}";
        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
        Subscription = subscription;
        BorrowRecord = borrowRecord;
    }

    public static void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new PaymentIsNullException(nameof(payment), "Cannot add null payment to extent");

        lock (_lockPayment)
        {
            if (string.IsNullOrWhiteSpace(payment.PaymentCode))
                throw new ArgumentException("PaymentCode cannot be empty");

            if (_allPayments.Any(p => p.Id == payment.Id))
                throw new PaymentAlreadyExistsException($"Payment with ID {payment.Id} already exists in extent");

            _allPayments.Add(payment);
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
        }
    }
}