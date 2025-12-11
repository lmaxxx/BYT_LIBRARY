using byt_library.Domain.Entities;
using byt_library.Domain.Enums;
using byt_library.Domain.Exceptions;
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
        Catalog.ClearCatalogExtent();
        Newspaper.ClearNewspaperExtent();
        OnlineMagazine.ClearOnlineMagazineExtent();
        Staff.ClearStaffExtent();
        Student.ClearStudentExtent();
        Translation.ClearTranslationExtent();
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
        Catalog.ClearCatalogExtent();
        Newspaper.ClearNewspaperExtent();
        OnlineMagazine.ClearOnlineMagazineExtent();
        Staff.ClearStaffExtent();
        Student.ClearStudentExtent();
        Translation.ClearTranslationExtent();
    }

    [Test]
    public void BorrowRecord_CalculateFineAmount_WhenOverdue_ReturnsCorrectFine()
    {
        var borrowRecord = new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"));

        borrowRecord.ReturnDate = borrowRecord.DueDate.AddDays(5);

        var fineAmount = borrowRecord.FineAmount;
        var calculatedFine = borrowRecord.CalculateFine();

        Assert.That(fineAmount, Is.EqualTo(5.0), "Fine should be $1 per day overdue");
        Assert.That(calculatedFine, Is.EqualTo(5.0), "CalculateFine() should return same as FineAmount property");
    }

    [Test]
    public void Payment_Constructor_WhenBothSubscriptionAndBorrowRecordProvided_ThrowsException()
    {
        var subscription = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1));
        var borrowRecord = new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"));

        var ex = Assert.Throws<PaymentXorViolationException>(() =>
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
        var ex = Assert.Throws<PaymentXorViolationException>(() =>
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
        var subscription = MakeSub(startDate, endDate);

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

        var futureSubscription = MakeSub(tomorrow, tomorrow.AddDays(30));
        Assert.That(futureSubscription.IsActive(), Is.False, "Subscription should be inactive before start date");

        var expiredSubscription = MakeSub(yesterday.AddDays(-30), yesterday);
        Assert.That(expiredSubscription.IsActive(), Is.False, "Subscription should be inactive after end date");

        var activeSubscription = MakeSub(yesterday, tomorrow);
        Assert.That(activeSubscription.IsActive(), Is.True, "Subscription should be active when within date range");

        var startingToday = MakeSub(today, tomorrow);
        Assert.That(startingToday.IsActive(), Is.True, "Subscription should be active on start date");

        var endingLater = MakeSub(yesterday, now.AddHours(1));
        Assert.That(endingLater.IsActive(), Is.True, "Subscription should be active when end date is after current time");

        var endedNow = MakeSub(yesterday, now.AddMinutes(-1));
        Assert.That(endedNow.IsActive(), Is.False, "Subscription should be inactive when end date has passed");
    }

    [Test]
    public void BorrowRecord_ReturnOnTime_FineAmountIsZero()
    {
        var borrowRecord = new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"));

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
        Book book = new Book(
            "978-3-16-148410-0",
            "Clean Code",
            "A Handbook of Agile Software Craftsmanship",
            true,
            464,
            "https://example.com/clean-code",
            CoverType.Hard,
            5
        );

        OnlineMagazine magazine = new OnlineMagazine(
            "https://newyorkmagazine.com/harvard-law-is-awful",
            "Harvard Law is Awful",
            "About Harvard law.",
            false,
            464,
            "https://example.com/harvard-law-is-awful"
        );

        Assert.Throws<UnsupportedLanguageException>(() => magazine.AddTranslation("german"));
        Assert.Throws<UnsupportedLanguageException>(() => magazine.AddTranslation("french"));
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

            var borrowRecord = new BorrowRecord(
                originalBorrowDate,
                originalDueDate,
                originalReturnDate,
                BorrowRecordStatus.Returned, 
                "BR-TEST123",
                new Student("Jakub", "Koko", DateTime.Now, DateTime.Now),
                new Newspaper("Nothing", "Nothing", "Nothing"),
                null
                );

            var borrowRecordList = new List<BorrowRecord> { borrowRecord };

            persistenceService.Save(borrowRecordList);
            BorrowRecord.ClearBorrowRecordExtent();
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

    [Test]
    public void Author_AddAuthor_WithDuplicateNickname_ThrowsAuthorWithSuchNicknameAlreadyExistsException()
    {
        new Author("John", "Doe", new DateTime(1995, 5, 5), "john.doe@example.com", "JD");

        Assert.Throws<AuthorWithSuchNicknameAlreadyExistsException>(() =>
        {
            new Author("Jane", "Doe", new DateTime(2000, 2, 2), "jane.doe@example.com", "JD");
        });
    }

    [Test]
    public void Author_Constructor_WithEmptyNickname_ThrowsNicknameIsEmptyException()
    {
        Assert.Throws<NicknameIsEmptyException>(() =>
        {
            new Author("John", "Doe", new DateTime(2012, 11, 7), "john.doe@example.com", " ");
        });
    }

    [Test]
    public void Book_Constructor_WithEmptyTitle_ThrowsTitleIsEmptyException()
    {
        Assert.Throws<TitleIsEmptyException>(() =>
        {
            new Book("978-0-307-74365-5", "", "Description");
        });
    }

    [Test]
    public void Book_Constructor_WithEmptyDescription_ThrowsDescriptionIsEmptyException()
    {
        Assert.Throws<DescriptionIsEmptyException>(() =>
        {
            new Book("978-0-307-74365-5", "Title", "");
        });
    }

    [Test]
    public void Book_Constructor_WithInvalidQuantity_ThrowsInvalidQuantityException()
    {
        Assert.Throws<InvalidQuantityException>(() =>
        {
            new Book("978-0-307-74365-5", "Title", "Description", quantity: 0);
        });
    }

    [Test]
    public void Book_AddBook_WithEmptyIsbn_ThrowsBookIsbnIsEmptyException()
    {
        Assert.Throws<BookISBNIsEmptyException>(() =>
        {
            new Book("", "Title", "Description");
        });
    }

    [Test]
    public void Book_AddBook_WithDuplicateIsbn_ThrowsBookAlreadyExistsException()
    {
        new Book("978-0-307-74365-5", "Title", "Description");

        Assert.Throws<BookAlreadyExistsException>(() =>
        {
            new Book("978-0-307-74365-5", "Title2", "Description2");
        });
    }

    [Test]
    public void BorrowRecord_Constructor_WithInvalidBorrowDays_ThrowsInvalidBorrowDaysException()
    {
        Assert.Throws<InvalidBorrowDaysException>(() => new BorrowRecord(0, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing")));
    }

    [Test]
    public void BorrowRecord_CancelBorrowRecordRequest_WhenActive_ThrowsBorrowRecordIsActiveException()
    {
        var borrowRecord = new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"));
        Assert.Throws<BorrowRecordIsActiveException>(() => borrowRecord.CancelBorrowRecordRequest());
    }

    [Test]
    public void BorrowRecord_ReturnBorrowRecord_WhenInactive_ThrowsBorrowRecordIsInactiveException()
    {
        var borrowRecord = new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"));
        borrowRecord.Status = BorrowRecordStatus.Returned;
        Assert.Throws<BorrowRecordIsInactiveException>(() => borrowRecord.ReturnBorrowRecord());
    }

    [Test]
    public void Catalog_Constructor_WithEmptyName_ThrowsCatalogIsEmptyException()
    {
        Assert.Throws<CatalogIsEmptyException>(() => new Catalog(""));
    }

    [Test]
    public void Catalog_AddCatalog_WithDuplicateName_ThrowsCatalogWithThisNameAlreadyExistsException()
    {
        new Catalog("Fiction");
        Assert.Throws<CatalogWithThisNameAlreadyExistsException>(() => new Catalog("Fiction"));
    }

    [Test]
    public void Newspaper_Constructor_WithEmptyDescription_ThrowsDescriptionIsEmptyException()
    {
        Assert.Throws<DescriptionIsEmptyException>(() => new Newspaper("Publisher", "Title", ""));
    }

    [Test]
    public void Newspaper_Constructor_WithInvalidQuantity_ThrowsInvalidQuantityException()
    {
        Assert.Throws<InvalidQuantityException>(() => new Newspaper("Publisher", "Title", "Description", quantity: 0));
    }

    [Test]
    public void Newspaper_Constructor_WithEmptyTitle_ThrowsTitleIsEmptyException()
    {
        Assert.Throws<TitleIsEmptyException>(() => new Newspaper("Publisher", "", "Description"));
    }

    [Test]
    public void Newspaper_Constructor_WithEmptyPublisher_ThrowsPublisherIsEmptyException()
    {
        Assert.Throws<PublisherIsEmptyException>(() => new Newspaper("", "Title", "Description"));
    }

    [Test]
    public void Newspaper_AddNewspaper_WithDuplicateNewspaper_ThrowsNewspaperAlreadyExistsException()
    {
        new Newspaper("Publisher", "Title", "Description");
        Assert.Throws<NewspaperAlreadyExistsException>(() => new Newspaper("Publisher", "Title", "Description"));
    }

    [Test]
    public void OnlineMagazine_Constructor_WithEmptyTitle_ThrowsTitleIsEmptyException()
    {
        Assert.Throws<TitleIsEmptyException>(() => new OnlineMagazine("link", "", "description"));
    }

    [Test]
    public void OnlineMagazine_Constructor_WithEmptyDescription_ThrowsDescriptionIsEmptyException()
    {
        Assert.Throws<DescriptionIsEmptyException>(() => new OnlineMagazine("link", "title", ""));
    }

    [Test]
    public void OnlineMagazine_Constructor_WithEmptyPageLink_ThrowsPageLinkIsEmptyException()
    {
        Assert.Throws<PageLinkIsEmptyException>(() => new OnlineMagazine("", "title", "description"));
    }

    [Test]
    public void OnlineMagazine_AddOnlineMagazine_WithDuplicatePageLink_ThrowsOnlineMagazineAlreadyExistsException()
    {
        new OnlineMagazine("link", "title", "description");
        Assert.Throws<OnlineMagazineAlreadyExistsException>(() => new OnlineMagazine("link", "title2", "description2"));
    }

    [Test]
    public void Payment_Constructor_WithInvalidAmount_ThrowsInvalidAmountException()
    {
        var subscription = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1));
        Assert.Throws<InvalidAmountException>(() => new Payment(0, DateTime.Now, PaymentMethod.Cash, subscription: subscription));
    }

    [Test]
    public void Person_Constructor_WithInvalidEmail_ThrowsInvalidEmailException()
    {
        Assert.Throws<InvalidEmailException>(() => new Person("John", "Doe", new DateTime(1990, 1, 1), "invalid-email"));
    }

    [Test]
    public void Person_Constructor_WithEmptyFirstName_ThrowsPersonFirstNameIsEmptyException()
    {
        Assert.Throws<PersonFirstNameIsEmptyException>(() => new Person("", "Doe", new DateTime(1987, 7, 7)));
    }

    [Test]
    public void Person_Constructor_WithEmptyLastName_ThrowsPersonLastNameIsEmptyException()
    {
        Assert.Throws<PersonLastNameIsEmptyException>(() => new Person("John", "", new DateTime(1987, 7, 7)));
    }

    [Test]
    public void Person_AddPerson_WithDuplicateName_ThrowsPersonAlreadyExistsException()
    {
        new Person("John", "Doe", new DateTime(1991, 1, 1));
        Assert.Throws<PersonAlreadyExistsException>(() => new Person("John", "Doe", new DateTime(1991, 1, 1)));
    }

    [Test]
    public void Staff_Constructor_WithEmptyDepartment_ThrowsDepartmentIsEmptyException()
    {
        Assert.Throws<DepartmentIsEmptyException>(() => new Staff("John", "Doe", new DateTime(1995, 5, 5), ""));
    }

    [Test]
    public void Staff_SetSupervisor_WithNullSupervisor_ThrowsStaffIsNullException()
    {
        var staff = new Staff("Bob", "Doe", new DateTime(2001, 1, 1), "IT");
        Assert.Throws<StaffIsNullException>(() => staff.SetSupervisor(null));
    }

    [Test]
    public void Staff_SetSupervisor_WithSelfAsSupervisor_ThrowsStaffSelfSupervisionException()
    {
        var staff = new Staff("John", "Doe", new DateTime(1990, 1, 1), "IT");
        Assert.Throws<StaffSelfSupervisionException>(() => staff.SetSupervisor(staff));
    }

    [Test]
    public void Student_Constructor_WithFutureEnrollmentDate_ThrowsInvalidEnrollmentDateException()
    {
        Assert.Throws<InvalidEnrollmentDateException>(() => new Student("John", "Doe", new DateTime(2005, 6, 7), DateTime.Now.AddDays(1)));
    }

    [Test]
    public void Student_AddSubscription_WithNullSubscription_ThrowsSubscriptionIsNullException()
    {
        var student = new Student("John", "Doe", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));
        Assert.Throws<SubscriptionIsNullException>(() => student.AddSubscription(null));
    }

    [Test]
    public void Student_AddSubscription_WithSubscriptionBelongingToAnotherStudent_ThrowsSubscriptionAlreadyBelongsException()
    {
        var student1 = new Student("Bob", "Doe", new DateTime(1990, 1, 1), new DateTime(2025, 10, 10));
        var student2 = new Student("Jane", "Doe", new DateTime(1991, 1, 1), new DateTime(2024, 3, 3));

        // subscription already associated with student1
        var subscription = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1), student1);

        Assert.Throws<SubscriptionAlreadyBelongsException>(() =>
            student2.AddSubscription(subscription)
        );
    }

    [Test]
    public void Student_UpdateSubscription_WithUnassignedSubscription_ThrowsSubscriptionIsNotAssignedException()
    {
        var student = new Student("John", "Kolins", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));

        var studentTemp = new Student("Temp", "T", new DateTime(1990,1,1), DateTime.Now);
        var oldSub = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1), studentTemp);

        var studentTemp2 = new Student("Temp2", "X", new DateTime(1990,1,2), DateTime.Now);
        var newSub = MakeSub(DateTime.Now, DateTime.Now.AddMonths(2), studentTemp2);

        // student has NO current subscription -> should throw
        Assert.Throws<SubscriptionIsNotAssignedException>(() => student.UpdateSubscription(newSub));
    }

    [Test]
    public void Subscription_Constructor_WithInvalidDateRange_ThrowsInvalidDateRangeException()
    {
        Assert.Throws<InvalidDateRangeException>(() => MakeSub(DateTime.Now, DateTime.Now.AddDays(-1)));
    }

    [Test]
    public void Subscription_SetStudent_WithNullStudent_ThrowsStudentIsNullException()
    {
        Assert.Throws<StudentIsNullException>(() => new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), null, null));
    }

    [Test]
    public void Subscription_SetStudent_WithAlreadyAssignedSubscription_ThrowsSubscriptionAlreadyBelongsException()
    {
        var student1 = new Student("John", "Doe", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));
        var student2 = new Student("Jane", "Doe", new DateTime(1991, 1, 1), new DateTime(2023, 1, 1));

        var subscription = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1), student1);

        Assert.Throws<SubscriptionAlreadyBelongsException>(() =>
            subscription.SetStudent(student2)
        );
    }

    [Test]
    public void Translation_CannotBeSharedBetweenOwners()
    {
        IDigitalResource book1 = new Book("ISBN1", "Book 1", "Desc", link: "http://book1.com");
        IDigitalResource book2 = new Book("ISBN2", "Book 2", "Desc", link: "http://book2.com");

        book1.AddTranslation("english");

        // Try to add same translation to different book - should create new translation
        book2.AddTranslation("english");

        // Verify each book has its own translation by querying the extent
        var allTranslations = Translation.GetAllTranslations();
        var book1Translations = allTranslations
            .Where(t => t.Owner is Book b && b.ISBN == ((Book)book1).ISBN)
            .ToList();
        var book2Translations = allTranslations
            .Where(t => t.Owner is Book b && b.ISBN == ((Book)book2).ISBN)
            .ToList();

        Assert.That(book1Translations.Count, Is.EqualTo(1));
        Assert.That(book2Translations.Count, Is.EqualTo(1));
        Assert.That(book1Translations[0], Is.Not.EqualTo(book2Translations[0]));
    }

    [Test]
    public void Book_RemoveBook_CascadesDeleteToTranslations()
    {
        IDigitalResource book = new Book("ISBN123", "Test Book", "Description", link: "http://book.com");
        book.AddTranslation("english");
        book.AddTranslation("polish");

        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(2));

        Book.RemoveBook("ISBN123");

        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(0));
    }

    [Test]
    public void OnlineMagazine_RemoveOnlineMagazine_CascadesDeleteToTranslations()
    {
        IDigitalResource magazine = new OnlineMagazine("http://mag.com", "Test Mag", "Description",
                                          link: "http://mag.com/content");
        magazine.AddTranslation("english");
        magazine.AddTranslation("ukrainian");

        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(2));

        OnlineMagazine.RemoveOnlineMagazine("http://mag.com");

        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(0));
    }

    [Test]
    public void Translation_HasOwnerReference()
    {
        IDigitalResource book = new Book("ISBN456", "Test Book", "Description", link: "http://book.com");
        book.AddTranslation("english");

        // Get the translation from extent by filtering by owner ISBN
        var translation = Translation.GetAllTranslations()
            .First(t => t.Owner is Book b && b.ISBN == "ISBN456");

        Assert.That(translation.Owner, Is.Not.Null);
        Assert.That(translation.Owner, Is.InstanceOf<Book>());
        Assert.That(((Book)translation.Owner).ISBN, Is.EqualTo("ISBN456"));
    }

    [Test]
    public void Translation_OwnerReference_PersistsAndResolves()
    {
        IDigitalResource book = new Book("ISBN789", "Test Book", "Description", link: "http://book.com");
        book.AddTranslation("polish");

        // Get the translation from extent (simulating reload)
        var translation = Translation.GetAllTranslations()[0];

        Assert.That(translation.Owner, Is.Not.Null);
        // Verify owner resolves correctly (OwnerId is private, test through Owner)
        Assert.That(translation.Owner is Book b && b.ISBN == "ISBN789", Is.True);
        Assert.That(translation.Owner, Is.InstanceOf<Book>());
    }

    [Test]
    public void Person_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalPerson = new Person("John", "Doe", new DateTime(1990, 1, 1), "john.doe@example.com");
            var personList = new List<Person> { originalPerson };

            persistenceService.Save(personList);
            Person.ClearExtent();
            var loadedPeople = persistenceService.Load<Person>();

            Assert.That(loadedPeople, Is.Not.Null);
            Assert.That(loadedPeople, Has.Count.EqualTo(1));

            var loadedPerson = loadedPeople[0];
            Assert.That(loadedPerson.FirstName, Is.EqualTo("John"));
            Assert.That(loadedPerson.LastName, Is.EqualTo("Doe"));
            Assert.That(loadedPerson.DateOfBirth, Is.EqualTo(new DateTime(1990, 1, 1)));
            Assert.That(loadedPerson.Email, Is.EqualTo("john.doe@example.com"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Author_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalAuthor = new Author("Ben", "White", new DateTime(1976, 12, 16), "ben.white@gmail.com", "Classic");
            var authorList = new List<Author> { originalAuthor };

            persistenceService.Save(authorList);
            Author.ClearAuthorExtent();
            Person.ClearExtent();
            var loadedAuthors = persistenceService.Load<Author>();

            Assert.That(loadedAuthors, Is.Not.Null);
            Assert.That(loadedAuthors, Has.Count.EqualTo(1));

            var loadedAuthor = loadedAuthors[0];
            Assert.That(loadedAuthor.FirstName, Is.EqualTo("Ben"));
            Assert.That(loadedAuthor.LastName, Is.EqualTo("White"));
            Assert.That(loadedAuthor.DateOfBirth, Is.EqualTo(new DateTime(1976, 12, 16)));
            Assert.That(loadedAuthor.Email, Is.EqualTo("ben.white@gmail.com"));
            Assert.That(loadedAuthor.Nickname, Is.EqualTo("Classic"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Student_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var enrollmentDate = new DateTime(2023, 9, 1);
            var originalStudent = new Student("Harry", "Potter", new DateTime(1980, 7, 31), enrollmentDate);
            var studentList = new List<Student> { originalStudent };

            persistenceService.Save(studentList);
            var loadedStudents = persistenceService.Load<Student>();

            Assert.That(loadedStudents, Is.Not.Null);
            Assert.That(loadedStudents, Has.Count.EqualTo(1));

            var loadedStudent = loadedStudents[0];
            Assert.That(loadedStudent.FirstName, Is.EqualTo("Harry"));
            Assert.That(loadedStudent.LastName, Is.EqualTo("Potter"));
            Assert.That(loadedStudent.DateOfBirth, Is.EqualTo(new DateTime(1980, 7, 31)));
            Assert.That(loadedStudent.EnrollmentDate, Is.EqualTo(enrollmentDate));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Staff_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            Staff.ClearStaffExtent();
            Person.ClearExtent();

            var supervisor = new Staff("Minerva", "McGonagall", new DateTime(1935, 10, 4), "Transfiguration");
            var originalStaff = new Staff("Albus", "Dumbledore", new DateTime(1881, 8, 1), "Headmaster");
            originalStaff.SetSupervisor(supervisor);

            var staffList = new List<Staff> { originalStaff, supervisor };

            persistenceService.Save(staffList);
            Staff.ClearStaffExtent();
            Person.ClearExtent();
            var loadedStaffList = persistenceService.Load<Staff>();

            Assert.That(loadedStaffList, Is.Not.Null);
            Assert.That(loadedStaffList, Has.Count.EqualTo(2));

            var loadedDumbledore = loadedStaffList.First(s => s.FirstName == "Albus");
            var loadedMcGonagall = loadedStaffList.First(s => s.FirstName == "Minerva");

            Assert.That(loadedDumbledore.FirstName, Is.EqualTo("Albus"));
            Assert.That(loadedDumbledore.LastName, Is.EqualTo("Dumbledore"));
            Assert.That(loadedDumbledore.Department, Is.EqualTo("Headmaster"));
            Assert.That(loadedDumbledore.GetSupervisor(), Is.Not.Null);
            Assert.That(loadedDumbledore.GetSupervisor().FirstName, Is.EqualTo("Minerva"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Book_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalBook = new Book(
                "978-0439708180",
                "Harry Potter and the Sorcerer's Stone",
                "First book in the series",
                true,
                500,
                "http://example.com/hp1",
                CoverType.Hard,
                10
            );
            var bookList = new List<Book> { originalBook };

            persistenceService.Save(bookList);
            Book.ClearBookExtent();
            var loadedBooks = persistenceService.Load<Book>();

            Assert.That(loadedBooks, Is.Not.Null);
            Assert.That(loadedBooks, Has.Count.EqualTo(1));

            var loadedBook = loadedBooks[0];
            Assert.That(loadedBook.ISBN, Is.EqualTo("978-0439708180"));
            Assert.That(loadedBook.Title, Is.EqualTo("Harry Potter and the Sorcerer's Stone"));
            Assert.That(loadedBook.Description, Is.EqualTo("First book in the series"));
            Assert.That(loadedBook.HasAudio, Is.True);
            Assert.That(loadedBook.Quantity, Is.EqualTo(10));
            Assert.That(loadedBook.Size, Is.EqualTo(500));
            Assert.That(loadedBook.Link, Is.EqualTo("http://example.com/hp1"));
            Assert.That(loadedBook.CoverType, Is.EqualTo(CoverType.Hard));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Catalog_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalCatalog = new Catalog("Fiction");
            var catalogList = new List<Catalog> { originalCatalog };

            persistenceService.Save(catalogList);
            Catalog.ClearCatalogExtent();
            var loadedCatalogs = persistenceService.Load<Catalog>();

            Assert.That(loadedCatalogs, Is.Not.Null);
            Assert.That(loadedCatalogs, Has.Count.EqualTo(1));

            var loadedCatalog = loadedCatalogs[0];
            Assert.That(loadedCatalog.Name, Is.EqualTo("Fiction"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Newspaper_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalNewspaper = new Newspaper("Daily Prophet", "War is Over", "Voldemort defeated", quantity: 100);
            var newspaperList = new List<Newspaper> { originalNewspaper };

            persistenceService.Save(newspaperList);
            var loadedNewspapers = persistenceService.Load<Newspaper>();

            Assert.That(loadedNewspapers, Is.Not.Null);
            Assert.That(loadedNewspapers, Has.Count.EqualTo(1));

            var loadedNewspaper = loadedNewspapers[0];
            Assert.That(loadedNewspaper.Publisher, Is.EqualTo("Daily Prophet"));
            Assert.That(loadedNewspaper.Title, Is.EqualTo("War is Over"));
            Assert.That(loadedNewspaper.Description, Is.EqualTo("Voldemort defeated"));
            Assert.That(loadedNewspaper.Quantity, Is.EqualTo(100));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void OnlineMagazine_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalMagazine = new OnlineMagazine(
                "https://witchweekly.com/latest",
                "Witch Weekly",
                "Latest issue",
                false,
                20
            );
            var magazineList = new List<OnlineMagazine> { originalMagazine };

            persistenceService.Save(magazineList);
            OnlineMagazine.ClearOnlineMagazineExtent();
            var loadedMagazines = persistenceService.Load<OnlineMagazine>();

            Assert.That(loadedMagazines, Is.Not.Null);
            Assert.That(loadedMagazines, Has.Count.EqualTo(1));

            var loadedMagazine = loadedMagazines[0];
            Assert.That(loadedMagazine.PageLink, Is.EqualTo("https://witchweekly.com/latest"));
            Assert.That(loadedMagazine.Title, Is.EqualTo("Witch Weekly"));
            Assert.That(loadedMagazine.Description, Is.EqualTo("Latest issue"));
            Assert.That(loadedMagazine.Size, Is.EqualTo(20));
            Assert.That(loadedMagazine.HasAudio, Is.False);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    /*[Test]
    public void Translation_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var originalTranslation = new Translation("http://example.com/hp1/pl", "Polish");
            var translationList = new List<Translation> { originalTranslation };

            persistenceService.Save(translationList);
            var loadedTranslations = persistenceService.Load<Translation>();

            Assert.That(loadedTranslations, Is.Not.Null);
            Assert.That(loadedTranslations, Has.Count.EqualTo(1));

            var loadedTranslation = loadedTranslations[0];
            Assert.That(loadedTranslation.Link, Is.EqualTo("http://example.com/hp1/pl"));
            Assert.That(loadedTranslation.Language, Is.EqualTo("Polish"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }*/

    [Test]
    public void Subscription_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);
            var originalSubscription = MakeSub(startDate, endDate);
            var subscriptionList = new List<Subscription> { originalSubscription };

            persistenceService.Save(subscriptionList);
            var loadedSubscriptions = persistenceService.Load<Subscription>();

            Assert.That(loadedSubscriptions, Is.Not.Null);
            Assert.That(loadedSubscriptions, Has.Count.EqualTo(1));

            var loadedSubscription = loadedSubscriptions[0];
            Assert.That(loadedSubscription.StartDate, Is.EqualTo(startDate));
            Assert.That(loadedSubscription.EndDate, Is.EqualTo(endDate));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void Payment_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var subscription = MakeSub(DateTime.Now, DateTime.Now.AddMonths(1));
            var originalPayment = new Payment(100, DateTime.Now, PaymentMethod.ByCard, subscription: subscription);
            var paymentList = new List<Payment> { originalPayment };

            persistenceService.Save(paymentList);
            var loadedPayments = persistenceService.Load<Payment>();

            Assert.That(loadedPayments, Is.Not.Null);
            Assert.That(loadedPayments, Has.Count.EqualTo(1));

            var loadedPayment = loadedPayments[0];
            Assert.That(loadedPayment.Amount, Is.EqualTo(100));
            Assert.That(loadedPayment.PaymentMethod, Is.EqualTo(PaymentMethod.ByCard));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }
    
    private Subscription MakeSub(DateTime start, DateTime end, Student? student = null)
    {
        if (student == null)
        {
            student = new Student("Auto", "Gen", new DateTime(1990, 1, 1), DateTime.Now);
        }

        var payment = new Payment(
            10,
            DateTime.Now,
            PaymentMethod.Cash,
            borrowRecord: new BorrowRecord(30, new Student("Jakub", "Koko", DateTime.Now, DateTime.Now), new Newspaper("Nothing", "Nothing", "Nothing"))); // safe for XOR

        return new Subscription(start, end, student, new[] { payment });
    }

    [Test]
    public void BorrowRecord_ReborrowResourceCorrectlyWorks()
    {
        // Allows duplicates
        var student = new Student("Auto", "Gen", new DateTime(1990, 1, 1), DateTime.Now);
        var newspaper = new Newspaper("Nothing", "nNothing", "Nothing");
        student.BorrowResource(newspaper);
        Assert.DoesNotThrow(() => student.BorrowResource(newspaper), "Student should allow to borrow the same resource.");
        
        // Maintains reverse association
        foreach (var record in BorrowRecord.GetAllBorrowRecords())
        {
            Assert.That(record.GetStudent() == student, "The only created records should have the association with the student.");
        }
    }
    
    [Test]
    public void Staff_SetSupervisor_AddsReverseSubordinateRelation()
    {
        var supervisor = new Staff("Alice", "Smith", new DateTime(1980, 1, 1), "IT");
        var worker = new Staff("Bob", "Jones", new DateTime(1990, 1, 1), "IT");

        worker.SetSupervisor(supervisor);

        Assert.That(worker.GetSupervisor(), Is.EqualTo(supervisor));
        Assert.That(supervisor.GetSubordinates(), Contains.Item(worker));
    }

    [Test]
    public void Staff_AddSubordinate_SetsSupervisorOnSubordinate()
    {
        var supervisor = new Staff("Alice", "Smith", new DateTime(1980, 1, 1), "IT");
        var worker = new Staff("Bob", "Jones", new DateTime(1990, 1, 1), "IT");

        supervisor.AddSubordinate(worker);

        Assert.That(worker.GetSupervisor(), Is.EqualTo(supervisor));
        Assert.That(supervisor.GetSubordinates(), Contains.Item(worker));
    }

    [Test]
    public void Staff_ChangeSupervisor_UpdatesBothOldAndNewRelations()
    {
        var oldSup = new Staff("Old", "Sup", new DateTime(1980, 1, 1), "HR");
        var newSup = new Staff("New", "Sup", new DateTime(1985, 1, 1), "HR");
        var worker = new Staff("Worker", "Guy", new DateTime(1990, 1, 1), "HR");

        worker.SetSupervisor(oldSup);
        worker.ChangeSupervisor(newSup);

        Assert.That(worker.GetSupervisor(), Is.EqualTo(newSup));
        Assert.That(oldSup.GetSubordinates(), Does.Not.Contain(worker));
        Assert.That(newSup.GetSubordinates(), Contains.Item(worker));
    }

    [Test]
    public void Student_AddSubscription_SetsReverseReference()
    {
        var student = new Student("Bob", "Doe", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-10));
        var payment = new Payment(10, DateTime.Now, PaymentMethod.Cash);
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student, new[] { payment });

        Assert.That(student.GetSubscription(), Is.EqualTo(subscription));
        Assert.That(subscription.GetStudent(), Is.EqualTo(student));
    }

    [Test]
    public void Student_AddSubscription_WhenAlreadyHasOne_ThrowsException()
    {
        var student = new Student("Bob", "Doe", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-10));

        var sub1 = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student,
            new[] { new Payment(20, DateTime.Now, PaymentMethod.Cash) });

        Assert.Throws<SubscriptionAlreadyBelongsException>(() => new Subscription(DateTime.Now, DateTime.Now.AddMonths(2), student,
            new[] { new Payment(20, DateTime.Now, PaymentMethod.Cash) }));
    }

    [Test]
    public void Subscription_AddPayment_SetsReverseReference()
    {
        var student = new Student("John", "Doe", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-10));

        var p1 = new Payment(10, DateTime.Now, PaymentMethod.Cash);
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student, new[] { p1 });

        var p2 = new Payment(5, DateTime.Now, PaymentMethod.Cash);
        subscription.AddPayment(p2);

        Assert.That(subscription.GetPayments(), Contains.Item(p2));
        Assert.That(p2.GetSubscription(), Is.EqualTo(subscription));
    }

    [Test]
    public void Subscription_RemovePayment_ClearsReverseReference()
    {
        var student = new Student("John", "Doe", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-10));
        var payment = new Payment(10, DateTime.Now, PaymentMethod.Cash);
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student, new[] { payment });

        subscription.RemovePayment(payment);

        Assert.That(subscription.GetPayments(), Does.Not.Contain(payment));
        Assert.That(payment.GetSubscription(), Is.Null);
    }
    
    private BorrowRecord MakeBorrowRecord()
    {
        var student = new Student("Borrow", "User", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-100));

        IResource resource = new Book(
            isbn: "111-222-333",
            title: "Test Resource",
            description: "A simple test book used for unit tests."
        );

        return new BorrowRecord(
            borrowDate: DateTime.Now.AddDays(-1),
            dueDate: DateTime.Now.AddDays(14),
            returnDate: null,
            status: BorrowRecordStatus.Ongoing,
            borrowCode: null,
            _student: student,
            _resource: resource,
            _payment: null
        );
    }


    [Test]
    public void Subscription_AddPayment_WhenPaymentBelongsToBorrowRecord_ThrowsException()
    {
        var student = new Student("John", "Doe", new DateTime(1990, 1, 1), DateTime.Now.AddDays(-10));
        var validPayment = new Payment(10, DateTime.Now, PaymentMethod.Cash);
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student, new[] { validPayment });

        var invalidPayment = new Payment(7, DateTime.Now, PaymentMethod.Cash, borrowRecord: MakeBorrowRecord());

        Assert.Throws<PaymentXorViolationException>(() => subscription.AddPayment(invalidPayment));
    }

    [Test]
    public void Payment_Constructor_XorViolation_ThrowsException()
    {
        var student = new Student("X", "Y", new DateTime(1990,1,1), DateTime.Now.AddDays(-5));
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student,
            new[] { new Payment(10, DateTime.Now, PaymentMethod.Cash) });

        var borrowRecord = MakeBorrowRecord();

        Assert.Throws<PaymentXorViolationException>(() => 
            new Payment(20, DateTime.Now, PaymentMethod.Cash, subscription, borrowRecord)
        );
    }

    [Test]
    public void Payment_AddSubscription_WhenAttachedToBorrowRecord_ThrowsException()
    {
        var borrowRecord = MakeBorrowRecord();
        var payment = new Payment(10, DateTime.Now, PaymentMethod.Cash, borrowRecord: borrowRecord);

        var student = new Student("Bob", "Test", new DateTime(1990,1,1), DateTime.Now.AddDays(-2));
        var sub = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student,
            new[] { new Payment(5, DateTime.Now, PaymentMethod.Cash) });

        Assert.Throws<PaymentXorViolationException>(() => payment.AddSubscription(sub));
    }

    [Test]
    public void Payment_AssignedThroughSubscriptionConstructor_SetsReverseRelation()
    {
        var student = new Student("Jim", "Guy", new DateTime(1990,1,1), DateTime.Now.AddDays(-1));
        var payment = new Payment(10, DateTime.Now, PaymentMethod.Cash);

        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1), student, new[] { payment });

        Assert.That(payment.GetSubscription(), Is.EqualTo(subscription));
        Assert.That(payment.GetBorrowRecord(), Is.Null);
    }

    [Test]
    public void
        Attempt_To_Remove_Not_Present_Resource_From_The_Catalog_Must_Throw_ResourceIsAlreadyPresentInTheCatalogException()
    {
        var catalog = new Catalog("Fiction");
        
        catalog.AddResource(new Book("978-0765387561",
            "Harry Potter",
            "A special collector's edition with author sign",
            true,
            450,
            "https://audible.com/addie-larue",
            CoverType.Hard,
            10,
            null
        ));
        
        Assert.Throws<ResourceIsAlreadyPresentInTheCatalogException>(() => 
            catalog.AddResource(new Book(
            "978-1250785596",
            "Harry Potter",
            "Travel-size pocket edition for comfortable reading",
            false,
            300,
            "https://store.localbooks.com/pocket-addie",
            CoverType.Soft,
            50,
            null
        )));
    }

    [Test]
    public void
        Attemt_To_Add_Existing_Resource_To_The_Catalog_Must_Throw_ResourceIsNotPresentInTheCatalogException()
    {
        var catalog = new Catalog("Fiction");

        Assert.Throws<ResourceIsNotPresentInTheCatalogException>(() => catalog.RemoveResource(
            new Book("978-0765387561",
                "Harry Potter",
                "A special collector's edition with author sign",
                true,
                450,
                "https://audible.com/addie-larue",
                CoverType.Hard,
                10,
                null
            )));
    }
}