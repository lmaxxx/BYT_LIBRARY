using byt_library.Domain.Enums;

namespace byt_library.Domain.Entities;

public class Payment
{
    public double Amount { get; }
    public DateTime PaymentDate { get; }
    public PaymentMethod PaymentMethod { get; }
    
    public Subscription? Subscription { get; }
    public BorrowRecord? BorrowRecord { get; }
    
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
}