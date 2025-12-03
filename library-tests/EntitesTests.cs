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

   /* [Test]
    public void Book_SaveAndLoad_PreservesAllProperties()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), "byt_library_test_" + Guid.NewGuid().ToString());
        var persistenceService = new JsonPersistenceService(testDirectory);

        try
        {
            var author = new Author("Stephen", "King", new DateTime(1947, 9, 21), "stephen.king@example.com", "The King");
            var book = new Book
            {
                ISBN = "978-0-307-74365-5",
                HasAudio = true,
                Quantity = 10,
                Size = 1138,
                Title = "The Stand",
                Description = "A post-apocalyptic horror novel.",
                Link = "https://example.com/the-stand",
                CoverType = CoverType.Hard,
                Translations = new List<string> { "English", "Polish" },
                Author = author
            };

            var bookList = new List<Book> { book };
            var authorList = new List<Author> { author };

            persistenceService.Save(bookList);
            persistenceService.Save(authorList); // Save author separately, as it's a separate extent

            var loadedBooks = persistenceService.Load<Book>();
            var loadedAuthors = persistenceService.Load<Author>();

            Assert.That(loadedBooks, Is.Not.Null, "Loaded books should not be null");
            Assert.That(loadedBooks, Has.Count.EqualTo(1), "Should load exactly one book");
            Assert.That(loadedAuthors, Is.Not.Null, "Loaded authors should not be null");
            Assert.That(loadedAuthors, Has.Count.EqualTo(1), "Should load exactly one author");

            var loadedBook = loadedBooks[0];
            var loadedAuthor = loadedAuthors[0];

            Assert.That(loadedBook.ISBN, Is.EqualTo(book.ISBN), "Book ISBN should match");
            Assert.That(loadedBook.HasAudio, Is.EqualTo(book.HasAudio), "Book HasAudio should match");
            Assert.That(loadedBook.Quantity, Is.EqualTo(book.Quantity), "Book Quantity should match");
            Assert.That(loadedBook.Size, Is.EqualTo(book.Size), "Book Size should match");
            Assert.That(loadedBook.Title, Is.EqualTo(book.Title), "Book Title should match");
            Assert.That(loadedBook.Description, Is.EqualTo(book.Description), "Book Description should match");
            Assert.That(loadedBook.Link, Is.EqualTo(book.Link), "Book Link should match");
            Assert.That(loadedBook.CoverType, Is.EqualTo(book.CoverType), "Book CoverType should match");
            Assert.That(loadedBook.Translations, Is.EquivalentTo(book.Translations), "Book Translations should match");

            Assert.That(loadedBook.Author.FirstName, Is.EqualTo(book.Author.FirstName), "Book Author FirstName should match");
            Assert.That(loadedBook.Author.LastName, Is.EqualTo(book.Author.LastName), "Book Author LastName should match");
            Assert.That(loadedBook.Author.Email, Is.EqualTo(book.Author.Email), "Book Author Email should match");
            Assert.That(loadedBook.Author.Nickname, Is.EqualTo(book.Author.Nickname), "Book Author Nickname should match");
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
    public void Author_AddAuthor_WithDuplicateName_ThrowsAuthorWithSuchNameAlreadyExistsException()
    {
        new Author("John", "Doe", new DateTime(1990, 1, 1), "john.doe@example.com", "JD");

        Assert.Throws<AuthorWithSuchNameAlreadyExistsException>(() =>
        {
            new Author("John", "Doe", new DateTime(1999, 7, 12), "john.doe2@example.com", "JD2");
        });
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
        Assert.Throws<InvalidBorrowDaysException>(() => new BorrowRecord(0));
    }

    [Test]
    public void BorrowRecord_CancelBorrowRecordRequest_WhenActive_ThrowsBorrowRecordIsActiveException()
    {
        var borrowRecord = new BorrowRecord();
        Assert.Throws<BorrowRecordIsActiveException>(() => borrowRecord.CancelBorrowRecordRequest());
    }

    [Test]
    public void BorrowRecord_ReturnBorrowRecord_WhenInactive_ThrowsBorrowRecordIsInactiveException()
    {
        var borrowRecord = new BorrowRecord();
        borrowRecord.Status = BorrowRecordStatus.Returned;
        Assert.Throws<BorrowRecordIsInactiveException>(() => borrowRecord.ReturnBorrowRecord());
    }

    [Test]
    public void BorrowRecord_AddBorrowRecord_WithDuplicateBorrowCode_ThrowsBorrowRecordAlreadyExistsException()
    {
        var borrowRecord1 = new BorrowRecord();
        Assert.Throws<BorrowRecordAlreadyExistsException>(() => {
            var borrowRecord2 = new BorrowRecord();
            borrowRecord2.BorrowCode = borrowRecord1.BorrowCode;
        });
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
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
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
    public void Staff_AddStaff_WithDuplicateStaff_ThrowsStaffAlreadyExistsException()
    {
        new Staff("John", "Doe", new DateTime(1977, 3, 10), "IT");
        Assert.Throws<StaffAlreadyExistsException>(() => new Staff("John", "Doe", new DateTime(1991, 1, 1), "HR"));
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
    public void Student_AddStudent_WithDuplicateStudent_ThrowsStudentAlreadyExistsException()
    {
        new Student("John", "Doe", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));
        Assert.Throws<StudentAlreadyExistsException>(() => new Student("John", "Doe", new DateTime(1991, 1, 1), new DateTime(2024, 1, 1)));
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
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        student1.AddSubscription(subscription);
        Assert.Throws<SubscriptionAlreadyBelongsException>(() => student2.AddSubscription(subscription));
    }

    [Test]
    public void Student_UpdateSubscription_WithUnassignedSubscription_ThrowsSubscriptionIsNotAssignedException()
    {
        var student = new Student("John", "Kolins", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));
        var oldSub = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        var newSub = new Subscription(DateTime.Now, DateTime.Now.AddMonths(2));
        Assert.Throws<SubscriptionIsNotAssignedException>(() => student.UpdateSubscription(oldSub, newSub));
    }

    [Test]
    public void Subscription_Constructor_WithInvalidDateRange_ThrowsInvalidDateRangeException()
    {
        Assert.Throws<InvalidDateRangeException>(() => new Subscription(DateTime.Now, DateTime.Now.AddDays(-1)));
    }

    [Test]
    public void Subscription_SetStudent_WithNullStudent_ThrowsStudentIsNullException()
    {
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        Assert.Throws<StudentIsNullException>(() => subscription.SetStudent(null));
    }

    [Test]
    public void Subscription_SetStudent_WithAlreadyAssignedSubscription_ThrowsSubscriptionAlreadyBelongsException()
    {
        var student1 = new Student("John", "Doe", new DateTime(1990, 1, 1), new DateTime(2023, 1, 1));
        var student2 = new Student("Jane", "Doe", new DateTime(1991, 1, 1), new DateTime(2023, 1, 1));
        var subscription = new Subscription(DateTime.Now, DateTime.Now.AddMonths(1));
        subscription.SetStudent(student1);
        Assert.Throws<SubscriptionAlreadyBelongsException>(() => subscription.SetStudent(student2));
    }

    [Test]
    public void Translation_Constructor_WithUnsupportedLanguage_ThrowsUnsupportedLanguageException()
    {
        Assert.Throws<UnsupportedLanguageException>(() => new Translation("link", "German"));
    }

    [Test]
    public void Translation_Constructor_WithEmptyLink_ThrowsLinkIsEmptyException()
    {
        Assert.Throws<LinkIsEmptyException>(() => new Translation("", "English"));
    }

    [Test]
    public void Translation_Constructor_WithEmptyLanguage_ThrowsLanguageIsEmptyException()
    {
        Assert.Throws<LanguageIsEmptyException>(() => new Translation("link", ""));
    }

    [Test]
    public void Translation_AddTranslation_WithDuplicateTranslation_ThrowsTranslationAlreadyExistsException()
    {
        new Translation("link", "English");
        Assert.Throws<TranslationAlreadyExistsException>(() => new Translation("link", "English"));
    }
}

