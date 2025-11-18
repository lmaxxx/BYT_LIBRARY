using byt_library.Domain.Entities;
using byt_library.Domain.Enums;

namespace library_tests;

public class EntitiesTests
{
    [SetUp]
    public void Setup()
    {
        Person.ClearExtent();
        Author.ClearAuthorExtent();
        BorrowRecord.ClearBorrowRecordExtent();
        Payment.ClearPaymentExtent();
        Subscription.ClearSubscriptionExtent();
        Book.ClearBookExtent();
    }

    [TearDown]
    public void TearDown()
    {
        Person.ClearExtent();
        Author.ClearAuthorExtent();
        BorrowRecord.ClearBorrowRecordExtent();
        Payment.ClearPaymentExtent();
        Subscription.ClearSubscriptionExtent();
        Book.ClearBookExtent();
    }

    [Test]
    public void BorrowRecord_CalculateFineAmount_WhenOverdue_ReturnsCorrectFine()
    {
        var borrowRecord = new BorrowRecord(borrowDays: 30);
        BorrowRecord.AddBorrowRecord(borrowRecord);

        borrowRecord.ReturnDate = borrowRecord.DueDate.AddDays(5);

        var fineAmount = borrowRecord.FineAmount;
        var calculatedFine = borrowRecord.CalculateFine();

        Assert.That(fineAmount, Is.EqualTo(5.0), "Fine should be $1 per day overdue");
        Assert.That(calculatedFine, Is.EqualTo(5.0), "CalculateFine() should return same as FineAmount property");
    }

    [Test]
    public void BorrowRecord_CancelBorrowRecordRequest_WhenOngoing_ThrowsInvalidOperationException()
    {
        var borrowRecord = new BorrowRecord();
        BorrowRecord.AddBorrowRecord(borrowRecord);

        var ex = Assert.Throws<InvalidOperationException>(() => borrowRecord.CancelBorrowRecordRequest());
        Assert.That(ex.Message, Does.Contain("Cannot cancel an active borrow record"));
    }

    [Test]
    public void Payment_Constructor_WhenBothSubscriptionAndBorrowRecordProvided_ThrowsException()
    {
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        var borrowRecord = new BorrowRecord();

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var payment = new Payment(
                amount: 50.0,
                paymentDate: DateTime.Now,
                paymentMethod: PaymentMethod.Cash,
                subscription: subscription,
                borrowRecord: borrowRecord
            );
        });

        Assert.That(ex.Message, Does.Contain("Payment must be attached to exactly one of Subscription or BorrowRecord"));
    }

    [Test]
    public void Payment_Constructor_WhenNeitherSubscriptionNorBorrowRecordProvided_ThrowsException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var payment = new Payment(
                amount: 50.0,
                paymentDate: DateTime.Now,
                paymentMethod: PaymentMethod.Cash,
                subscription: null,
                borrowRecord: null
            );
        });

        Assert.That(ex.Message, Does.Contain("Payment must be attached to exactly one of Subscription or BorrowRecord"));
    }

    [Test]
    public void Subscription_CalculateCost_WithPartialMonths_RoundsUpCorrectly()
    {
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 2, 15); // 45 days
        var subscription = new Subscription(startDate, endDate);

        var cost = subscription.CalculateCost();

        Assert.That(cost, Is.EqualTo(50.0), "Partial months should round up (45 days = 2 months)");
    }

    [Test]
    public void Subscription_IsActive_OnBoundaryDates_ReturnsCorrectStatus()
    {
        var now = DateTime.Now;
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        var futureSubscription = new Subscription(tomorrow, tomorrow.AddDays(30));
        Assert.That(futureSubscription.IsActive(), Is.False, "Subscription should be inactive before start date");

        var expiredSubscription = new Subscription(yesterday.AddDays(-30), yesterday);
        Assert.That(expiredSubscription.IsActive(), Is.False, "Subscription should be inactive after end date");
        
        var activeSubscription = new Subscription(yesterday, tomorrow);
        Assert.That(activeSubscription.IsActive(), Is.True, "Subscription should be active when within date range");

        var startingToday = new Subscription(today, tomorrow);
        Assert.That(startingToday.IsActive(), Is.True, "Subscription should be active on start date");

        var endingLater = new Subscription(yesterday, now.AddHours(1));
        Assert.That(endingLater.IsActive(), Is.True, "Subscription should be active when end date is after current time");

        var endedNow = new Subscription(yesterday, now.AddMinutes(-1));
        Assert.That(endedNow.IsActive(), Is.False, "Subscription should be inactive when end date has passed");
    }

    [Test]
    public void BorrowRecord_ReturnOnTime_FineAmountIsZero()
    {
        var borrowRecord = new BorrowRecord(borrowDays: 30);
        BorrowRecord.AddBorrowRecord(borrowRecord);

        borrowRecord.ReturnDate = borrowRecord.DueDate;
        var fineAmount = borrowRecord.FineAmount;

        Assert.That(fineAmount, Is.EqualTo(0.0), "No fine should be charged when returned on due date");

        borrowRecord.ReturnDate = borrowRecord.DueDate.AddDays(-5);
        var fineAmountEarly = borrowRecord.FineAmount;

        Assert.That(fineAmountEarly, Is.EqualTo(0.0), "No fine should be charged when returned early");
    }

    [Test]
    public void Person_AddPerson_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var person1 = new Person("John", "Doe", new DateTime(1990, 1, 1), "john.doe@example.com");
        Person.AddPerson(person1);

        var person2 = new Person("Jane", "Smith", new DateTime(1992, 5, 15), "JOHN.DOE@EXAMPLE.COM");

        var ex = Assert.Throws<InvalidOperationException>(() => Person.AddPerson(person2));
        Assert.That(ex.Message, Does.Contain("Person with email"));
        Assert.That(ex.Message, Does.Contain("already exists"));
    }

    [Test]
    public void Author_AddAuthor_WithDuplicateNickname_ThrowsInvalidOperationException()
    {
        var author1 = new Author("Stephen", "King", new DateTime(1947, 9, 21), "stephen@example.com", "The Master of Horror");
        Author.AddAuthor(author1);

        var author2 = new Author("Richard", "Bachman", new DateTime(1950, 1, 1), "richard@example.com", "the master of horror");

        var ex = Assert.Throws<InvalidOperationException>(() => Author.AddAuthor(author2));
        Assert.That(ex.Message, Does.Contain("Author with nickname"));
        Assert.That(ex.Message, Does.Contain("already exists"));
    }

    [Test]
    public void Person_Age_CalculatedCorrectly_ConsideringBirthdayThisYear()
    {
        var today = DateTime.Today;
        var currentYear = today.Year;

        var birthdayToday = new Person("John", "Doe", today.AddYears(-30));
        Assert.That(birthdayToday.Age, Is.EqualTo(30), "Age should be 30 when birthday is today");

        var birthdayLater = new Person("Jane", "Smith", new DateTime(currentYear - 30, today.Month, today.Day).AddDays(1));
        Assert.That(birthdayLater.Age, Is.EqualTo(29), "Age should be 29 when birthday hasn't occurred yet this year");

        var birthdayPassed = new Person("Bob", "Johnson", new DateTime(currentYear - 30, today.Month, today.Day).AddDays(-1));
        Assert.That(birthdayPassed.Age, Is.EqualTo(30), "Age should be 30 when birthday already passed this year");

        var infant = new Person("Baby", "Doe", new DateTime(currentYear, 1, 1));
        Assert.That(infant.Age, Is.EqualTo(0), "Age should be 0 for infant born this year");
    }

    [Test]
    public void Book_AddBook_WithValidProperties_CreatesValidInstance()
    {
        var book = new Book(
            ISBN: "978-3-16-148410-0",
            hasAudio: true,
            title: "Clean Code",
            description: "A Handbook of Agile Software Craftsmanship",
            coverType: CoverType.Hard,
            quantity: 5,
            size: 464,
            link: "https://example.com/clean-code"
        )
        {
            Title = "Clean Code",
            Description = "A Handbook of Agile Software Craftsmanship",
            Link = "https://example.com/clean-code",
            CoverType = CoverType.Hard,
            Translations = new List<Translation>()
        };

        Book.AddBook(book);

        Assert.That(Book.GetBookCount(), Is.EqualTo(1), "Book should be added to extent");
        var retrievedBook = Book.GetBookById(book.Id);
        Assert.That(retrievedBook, Is.Not.Null, "Book should be retrievable by ID");
        Assert.That(retrievedBook!.Title, Is.EqualTo("Clean Code"), "Title should match");
        Assert.That(retrievedBook.HasAudio, Is.True, "HasAudio property should be set");
        Assert.That(retrievedBook.Quantity, Is.EqualTo(5), "Quantity should be 5");
        Assert.That(retrievedBook.Id, Is.GreaterThan(0), "ID should be auto-generated");
    }
}