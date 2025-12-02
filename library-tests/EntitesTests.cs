using byt_library.Domain.Entities;
using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;

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

        borrowRecord.ReturnDate = borrowRecord.DueDate.AddDays(5);

        var fineAmount = borrowRecord.FineAmount;
        var calculatedFine = borrowRecord.CalculateFine();

        Assert.That(fineAmount, Is.EqualTo(5.0), "Fine should be $1 per day overdue");
        Assert.That(calculatedFine, Is.EqualTo(5.0), "CalculateFine() should return same as FineAmount property");
    }
    
    [Test]
    public void Payment_Constructor_WhenBothSubscriptionAndBorrowRecordProvided_ThrowsException()
    {
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        var borrowRecord = new BorrowRecord();

        var ex = Assert.Throws<PaymentIsNotAttachedException>(() =>
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
        var ex = Assert.Throws<PaymentIsNotAttachedException>(() =>
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

        borrowRecord.ReturnDate = borrowRecord.DueDate;
        var fineAmount = borrowRecord.FineAmount;

        Assert.That(fineAmount, Is.EqualTo(0.0), "No fine should be charged when returned on due date");

        borrowRecord.ReturnDate = borrowRecord.DueDate.AddDays(-5);
        var fineAmountEarly = borrowRecord.FineAmount;

        Assert.That(fineAmountEarly, Is.EqualTo(0.0), "No fine should be charged when returned early");
    }

    [Test]
    public void Person_AddPerson_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var person1 = new Person("John", "Doe", new DateTime(1990, 1, 1), "john.doe@example.com");

        var ex = Assert.Throws<PersonAlreadyExistsException>(() =>
            new Person("JOHN", "DOE", new DateTime(1992, 5, 15), "jane.smith@example.com"));
        Assert.That(ex.Message, Does.Contain("Person with name"));
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
    public void IDigitalResource_AddTranslation_WithLanguage_ThrowsNotSupportedExceptionWhenExpected()
    {
        IDigitalResource book = new Book{
            ISBN = "978-3-16-148410-0",
            HasAudio = true,
            Quantity = 5,
            Size = 464,
            Title = "Clean Code",
            Description = "A Handbook of Agile Software Craftsmanship",
            Link = "https://example.com/clean-code",
            CoverType = CoverType.Hard,
            Translations = []
        };

        IDigitalResource magazine = new OnlineMagazine()
        {
            Title = "Harvard Law is Awful",
            Description = "About Harvard law.",
            Link = "https://example.com/harvard-law-is-awful",
            Size = 464,
            PageLink = "https://newyorkmagazine.com/harvard-law-is-awful",
            Translations = [],
            HasAudio = false
        };

        Assert.Throws<NotSupportedException>(() => magazine.AddTranslation("german"));
        Assert.Throws<NotSupportedException>(() => magazine.AddTranslation("french"));
        Assert.DoesNotThrow(() => magazine.AddTranslation("english"));
        Assert.DoesNotThrow(() => magazine.AddTranslation("polish"));
        Assert.DoesNotThrow(() => magazine.AddTranslation("ukrainian"));
    }

    [Test]
    public void Author_AddAuthor_WithDuplicateName_ThrowsPersonAlreadyExistsException()
    {
        var author1 = new Author("Stephen", "King", new DateTime(1947, 9, 21), "stephen.king@example.com", "The King");

        var ex = Assert.Throws<PersonAlreadyExistsException>(() =>
            new Author("STEPHEN", "KING", new DateTime(1950, 1, 1), "another.email@example.com", "Different Nickname"));
        Assert.That(ex.Message, Does.Contain("Person with name"));
        Assert.That(ex.Message, Does.Contain("STEPHEN KING"));
    }

    [Test]
    public void BorrowRecord_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalBorrowDate = new DateTime(2025, 1, 15, 10, 30, 0);
            var originalDueDate = originalBorrowDate.AddDays(30);
            var originalReturnDate = originalDueDate.AddDays(5);

            var borrowRecord = new BorrowRecord
            {
                BorrowDate = originalBorrowDate,
                DueDate = originalDueDate,
                ReturnDate = originalReturnDate,
                Status = BorrowRecordStatus.Returned,
                BorrowCode = "BR-TEST123"
            };

            var borrowRecordList = new List<BorrowRecord> { borrowRecord };

            persistenceService.Save(borrowRecordList);

            var loadedBorrowRecords = persistenceService.Load<BorrowRecord>();

            Assert.That(loadedBorrowRecords, Is.Not.Null, "Loaded records should not be null");
            Assert.That(loadedBorrowRecords, Has.Count.EqualTo(1), "Should load exactly one borrow record");

            var loadedRecord = loadedBorrowRecords[0];
            Assert.That(loadedRecord.BorrowCode, Is.EqualTo("BR-TEST123"), "BorrowCode should match");
            Assert.That(loadedRecord.BorrowDate, Is.EqualTo(originalBorrowDate), "BorrowDate should match");
            Assert.That(loadedRecord.DueDate, Is.EqualTo(originalDueDate), "DueDate should match");
            Assert.That(loadedRecord.ReturnDate, Is.EqualTo(originalReturnDate), "ReturnDate should match");
            Assert.That(loadedRecord.Status, Is.EqualTo(BorrowRecordStatus.Returned), "Status should match");
            Assert.That(loadedRecord.FineAmount, Is.EqualTo(5.0), "FineAmount should be calculated correctly");
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }
}
