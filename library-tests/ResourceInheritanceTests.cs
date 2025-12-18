using System.Reflection;
using NUnit.Framework;
using byt_library.Domain.Entities;
using byt_library.Domain.Enums;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;

namespace library_tests;

public class ResourceInheritanceTests
{
    private int _testCounter = 0;

    [SetUp]
    public void Setup()
    {
        _testCounter = 0;
        Person.ClearExtent();
        Author.ClearAuthorExtent();
        Book.ClearBookExtent();
        OnlineMagazine.ClearOnlineMagazineExtent();
        Newspaper.ClearNewspaperExtent();
        Translation.ClearTranslationExtent();
    }

    [TearDown]
    public void TearDown()
    {
        Person.ClearExtent();
        Author.ClearAuthorExtent();
        Book.ClearBookExtent();
        OnlineMagazine.ClearOnlineMagazineExtent();
        Newspaper.ClearNewspaperExtent();
        Translation.ClearTranslationExtent();
    }

    public Resource CreateTestResource(string title = null)
    {
        var uniqueId = $"{_testCounter++}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        title = title ?? $"TestResource_{uniqueId}";
        var authors = new List<Author>();
        for (int i = 0; i < 2; i++)
        {
            var personId = $"{uniqueId}_{i}";
            var person = new Person($"Author{personId}", $"Last{personId}", DateTime.Now.AddYears(-30));
            authors.Add(new Author(person, $"Pen{personId}"));
        }
        return new Resource(title, "Test Description", authors);
    }

    public Book CreateTestBook(string isbn = null, string title = null)
    {
        isbn = isbn ?? GetUniqueISBN();
        title = title ?? GetUniqueTitle();
        var resource = CreateTestResource(title);
        return new Book(resource, isbn, title, "Test Description", link: "http://test.com");
    }

    public Book CreateFullBook(string isbn = null, CoverType coverType = CoverType.Hard, int quantity = 10)
    {
        isbn = isbn ?? GetUniqueISBN();
        var resource = CreateTestResource($"FullBook_{_testCounter}");
        return new Book(
            resource,
            isbn,
            $"Full Book {_testCounter}",
            "Complete Description",
            hasAudio: true,
            size: 500,
            link: "http://fullbook.com",
            coverType: coverType,
            quantity: quantity
        );
    }

    public OnlineMagazine CreateTestOnlineMagazine(string pageLink = null)
    {
        pageLink = pageLink ?? $"http://magazine.com/{_testCounter}";
        var resource = CreateTestResource($"Magazine Title {_testCounter}");
        return new OnlineMagazine(
            resource,
            pageLink,
            $"Test Magazine {_testCounter}",
            "Magazine Description",
            hasAudio: false,
            size: 200,
            link: $"http://content.com/{_testCounter}"
        );
    }

    public Newspaper CreateTestNewspaper(string title = null, string publisher = null)
    {
        title = title ?? $"Test Newspaper {_testCounter}";
        publisher = publisher ?? $"Test Publisher {_testCounter}";
        var resource = CreateTestResource(title);
        return new Newspaper(resource, publisher, title, "News Description", quantity: 50);
    }

    public void AssertResourceHasBothAssignments(Resource resource)
    {
        var digitalResource = resource.GetType().GetField("_digitalResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);
        var printedResource = resource.GetType().GetField("_printedResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);

        Assert.That(digitalResource, Is.Not.Null, "Digital resource should be assigned");
        Assert.That(printedResource, Is.Not.Null, "Printed resource should be assigned");
    }

    public void AssertResourceHasOnlyDigital(Resource resource)
    {
        var digitalResource = resource.GetType().GetField("_digitalResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);
        var printedResource = resource.GetType().GetField("_printedResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);

        Assert.That(digitalResource, Is.Not.Null, "Digital resource should be assigned");
        Assert.That(printedResource, Is.Null, "Printed resource should NOT be assigned");
    }

    public void AssertResourceHasOnlyPrinted(Resource resource)
    {
        var digitalResource = resource.GetType().GetField("_digitalResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);
        var printedResource = resource.GetType().GetField("_printedResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);

        Assert.That(digitalResource, Is.Null, "Digital resource should NOT be assigned");
        Assert.That(printedResource, Is.Not.Null, "Printed resource should be assigned");
    }

    public Dictionary<string, int> GetAllExtentCounts()
    {
        return new Dictionary<string, int>
        {
            ["Book"] = Book.GetAllBooks().Count,
            ["OnlineMagazine"] = OnlineMagazine.GetAllOnlineMagazines().Count,
            ["Newspaper"] = Newspaper.GetAllNewspapers().Count,
            ["Translation"] = Translation.GetAllTranslations().Count,
            ["Person"] = Person.GetAllPersons().Count,
            ["Author"] = Author.GetAllAuthors().Count
        };
    }

    private string GetUniqueISBN()
    {
        return $"978-{_testCounter++:D10}";
    }

    private string GetUniqueTitle()
    {
        return $"TestBook_{_testCounter++}";
    }

    [Test]
    public void Resource_AssignDigitalResource_FirstTime_ShouldSucceed()
    {
        var resource = CreateTestResource();

        var magazine = new OnlineMagazine(
            resource,
            "http://test.com/page",
            "Test Magazine",
            "Description",
            hasAudio: false,
            size: 100,
            link: "http://test.com"
        );

        AssertResourceHasOnlyDigital(resource);
    }

    [Test]
    public void Resource_AssignDigitalResource_SecondTime_ShouldThrowException()
    {
        var resource = CreateTestResource();
        var book = new Book(
            resource,
            GetUniqueISBN(),
            "Test Book",
            "Description",
            link: "http://test.com"
        );

        Assert.Throws<ResourceAlreadyHaveChildClassException>(() =>
        {
            var magazine = new OnlineMagazine(
                resource,
                "http://test.com",
                "Test Magazine",
                "Description",
                hasAudio: false,
                size: 100,
                link: "http://content.com"
            );
        });
    }

    [Test]
    public void Resource_AssignPrintedResource_FirstTime_ShouldSucceed()
    {
        var resource = CreateTestResource();

        var newspaper = new Newspaper(resource, "Publisher", "Newspaper Title", "Description", quantity: 50);

        AssertResourceHasOnlyPrinted(resource);
    }

    [Test]
    public void Resource_AssignPrintedResource_SecondTime_ShouldThrowException()
    {
        var resource = CreateTestResource();
        var newspaper1 = new Newspaper(resource, "Publisher1", "Newspaper1", "Description1", quantity: 50);

        Assert.Throws<ResourceAlreadyHaveChildClassException>(() =>
        {
            var newspaper2 = new Newspaper(resource, "Publisher2", "Newspaper2", "Description2", quantity: 30);
        });
    }

    [Test]
    public void Resource_AssignBothDigitalAndPrinted_WithBook_ShouldSucceed()
    {
        var resource = CreateTestResource();

        var book = new Book(
            resource,
            GetUniqueISBN(),
            "Test Book",
            "Description",
            hasAudio: true,
            size: 500,
            link: "http://book.com",
            coverType: CoverType.Hard,
            quantity: 10
        );

        AssertResourceHasBothAssignments(resource);
    }

    [Test]
    public void Book_ExtendsPrintedResource_CanAccessAllProperties()
    {
        var book = CreateFullBook(coverType: CoverType.Hard, quantity: 15);

        Assert.That(book.CoverType, Is.EqualTo(CoverType.Hard));
        Assert.That(book.Quantity, Is.EqualTo(15));

        Assert.That(book, Is.InstanceOf<PrintedResource>());
    }

    [Test]
    public void Book_Constructor_CallsBothAssignMethods_InCorrectOrder()
    {
        var resource = CreateTestResource();

        var book = new Book(
            resource,
            GetUniqueISBN(),
            "Test Book",
            "Description",
            link: "http://book.com"
        );

        AssertResourceHasBothAssignments(resource);

        Assert.That(Book.GetAllBooks().Count, Is.EqualTo(1));
    }

    [Test]
    public void Book_CanAddTranslation_ViaIDigitalResourceInterface()
    {
        var book = CreateTestBook();
        IDigitalResource digitalResource = book;

        digitalResource.AddTranslation("polish");
        digitalResource.AddTranslation("english");

        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(2));

        var translations = Translation.GetAllTranslations();
        Assert.That(translations.Any(t => t.Language == "polish"), Is.True);
        Assert.That(translations.Any(t => t.Language == "english"), Is.True);
    }

    [Test]
    public void Book_CanBeBothDigitalAndPrinted_OverlappingInheritance()
    {
        var resource = CreateTestResource();

        var book = new Book(
            resource,
            GetUniqueISBN(),
            "Hybrid Book",
            "Description",
            hasAudio: true,
            size: 600,
            link: "http://hybrid.com",
            coverType: CoverType.Soft,
            quantity: 25
        );

        AssertResourceHasBothAssignments(resource);

        Assert.That(book, Is.InstanceOf<IDigitalResource>());
        Assert.That(book, Is.InstanceOf<PrintedResource>());

        Assert.That(book.Size, Is.EqualTo(600));
        Assert.That(book.Quantity, Is.EqualTo(25));
    }

    [Test]
    public void MultipleBooks_ShareNoResourceState()
    {
        var resource1 = CreateTestResource("Resource1");
        var resource2 = CreateTestResource("Resource2");

        var book1 = new Book(resource1, GetUniqueISBN(), "Book1", "Desc1", link: "http://book1.com");
        var book2 = new Book(resource2, GetUniqueISBN(), "Book2", "Desc2", link: "http://book2.com");

        AssertResourceHasBothAssignments(resource1);
        AssertResourceHasBothAssignments(resource2);

        Assert.That(resource1, Is.Not.EqualTo(resource2));
        Assert.That(book1.ISBN, Is.Not.EqualTo(book2.ISBN));
    }

    [Test]
    public void OnlineMagazine_DigitalOnly_CannotBePrinted()
    {
        var resource = CreateTestResource();

        var magazine = new OnlineMagazine(
            resource,
            "http://magazine.com/page",
            "Digital Magazine",
            "Description",
            hasAudio: false,
            size: 300,
            link: "http://content.com"
        );

        AssertResourceHasOnlyDigital(resource);
    }

    [Test]
    public void OnlineMagazine_CannotCoexistWithPrintedOnSameResource()
    {
        var resource = CreateTestResource();
        var magazine = new OnlineMagazine(
            resource,
            "http://magazine.com/page",
            "Digital Magazine",
            "Description",
            hasAudio: false,
            size: 300,
            link: "http://content.com"
        );

        var newspaper = new Newspaper(resource, "Publisher", "Newspaper", "Description", quantity: 10);

        AssertResourceHasBothAssignments(resource);
    }

    [Test]
    public void OnlineMagazine_DisjointConstraint_EnforcedAtResourceLevel()
    {
        var resource = CreateTestResource();

        var magazine = new OnlineMagazine(
            resource,
            "http://page.com",
            "Magazine",
            "Desc",
            hasAudio: false,
            size: 200,
            link: "http://link.com"
        );

        var digitalResource = resource.GetType().GetField("_digitalResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);
        var printedResource = resource.GetType().GetField("_printedResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);

        Assert.That(digitalResource, Is.Not.Null);
        Assert.That(digitalResource, Is.InstanceOf<OnlineMagazine>());
        Assert.That(printedResource, Is.Null);
    }

    [Test]
    public void Newspaper_PrintedOnly_CannotBeDigital()
    {
        var resource = CreateTestResource();

        var newspaper = new Newspaper(
            resource,
            "The Daily News",
            "Newspaper Title",
            "Description",
            quantity: 100
        );

        AssertResourceHasOnlyPrinted(resource);
    }

    [Test]
    public void Newspaper_CannotCoexistWithDigitalOnSameResource()
    {
        var resource = CreateTestResource();
        var newspaper = new Newspaper(
            resource,
            "The Daily News",
            "Newspaper Title",
            "Description",
            quantity: 100
        );

        var magazine = new OnlineMagazine(
            resource,
            "http://page.com",
            "Magazine",
            "Desc",
            hasAudio: false,
            size: 200,
            link: "http://link.com"
        );

        AssertResourceHasBothAssignments(resource);
    }

    [Test]
    public void Newspaper_DisjointConstraint_EnforcedAtResourceLevel()
    {
        var resource = CreateTestResource();

        var newspaper = new Newspaper(
            resource,
            "Publisher",
            "News",
            "Description",
            quantity: 75
        );

        var digitalResource = resource.GetType().GetField("_digitalResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);
        var printedResource = resource.GetType().GetField("_printedResource",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(resource);

        Assert.That(printedResource, Is.Not.Null);
        Assert.That(printedResource, Is.InstanceOf<Newspaper>());
        Assert.That(digitalResource, Is.Null);
    }

    [Test]
    public void Resource_Delete_CascadesToBook_AndTranslations()
    {
        var bookISBN = GetUniqueISBN();
        var book = CreateTestBook(isbn: bookISBN, title: "BookToDelete");
        book.AddTranslation("polish");
        book.AddTranslation("english");

        var countsBefore = GetAllExtentCounts();

        var deleted = Book.RemoveBook(bookISBN);

        Assert.That(deleted, Is.True);

        var countsAfter = GetAllExtentCounts();
        Assert.That(countsAfter["Book"], Is.EqualTo(0));
        Assert.That(countsAfter["Translation"], Is.EqualTo(0));
    }

    [Test]
    public void Resource_Delete_CascadesToOnlineMagazine_AndTranslations()
    {
        var pageLink = $"http://magazine.com/unique_{Guid.NewGuid()}";
        var magazine = CreateTestOnlineMagazine(pageLink);
        magazine.AddTranslation("ukrainian");

        var deleted = OnlineMagazine.RemoveOnlineMagazine(pageLink);

        Assert.That(deleted, Is.True);
        Assert.That(OnlineMagazine.GetAllOnlineMagazines().Count, Is.EqualTo(0));
        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(0));
    }

    [Test]
    public void Resource_Delete_CascadesToNewspaper()
    {
        var title = $"NewspaperToDelete_{Guid.NewGuid()}";
        var publisher = "TestPublisher";
        var newspaper = CreateTestNewspaper(title, publisher);

        var deleted = Newspaper.RemoveNewspaper(title, publisher);

        Assert.That(deleted, Is.True);
        Assert.That(Newspaper.GetAllNewspapers().Count, Is.EqualTo(0));
    }

    [Test]
    public void Resource_Delete_WithBook_CascadesBothPaths()
    {
        var bookISBN = GetUniqueISBN();
        var book = new Book(
            CreateTestResource(),
            bookISBN,
            "Test Book",
            "Description",
            hasAudio: true,
            size: 500,
            link: "http://book.com",
            coverType: CoverType.Hard,
            quantity: 10
        );
        book.AddTranslation("polish");
        book.AddTranslation("english");

        var deleted = Book.RemoveBook(bookISBN);

        Assert.That(deleted, Is.True);

        Assert.That(Book.GetAllBooks().Count, Is.EqualTo(0), "Book should be removed (both paths)");
        Assert.That(Translation.GetAllTranslations().Count, Is.EqualTo(0), "Translations should be removed");
    }

    [Test]
    public void CascadeChain_Resource_Book_Translations_AllRemoved()
    {
        var bookISBN = GetUniqueISBN();
        var book = CreateTestBook(isbn: bookISBN, title: "CascadeBook");
        book.AddTranslation("polish");
        book.AddTranslation("english");
        book.AddTranslation("ukrainian");

        var countsBefore = GetAllExtentCounts();
        Assert.That(countsBefore["Book"], Is.EqualTo(1));
        Assert.That(countsBefore["Translation"], Is.EqualTo(3));

        var deleted = Book.RemoveBook(bookISBN);

        Assert.That(deleted, Is.True);

        var countsAfter = GetAllExtentCounts();
        Assert.That(countsAfter["Book"], Is.EqualTo(0), "All Books removed");
        Assert.That(countsAfter["Translation"], Is.EqualTo(0), "All Translations removed");
    }

    [Test]
    public void BookExtent_AfterCascadeDelete_RemainsConsistent()
    {
        var isbn1 = GetUniqueISBN();
        var isbn2 = GetUniqueISBN();
        var isbn3 = GetUniqueISBN();

        var book1 = CreateTestBook(isbn: isbn1, title: "Book1");
        var book2 = CreateTestBook(isbn: isbn2, title: "Book2");
        var book3 = CreateTestBook(isbn: isbn3, title: "Book3");

        Assert.That(Book.GetAllBooks().Count, Is.EqualTo(3));

        var deleted = Book.RemoveBook(isbn2);

        Assert.That(deleted, Is.True);
        Assert.That(Book.GetAllBooks().Count, Is.EqualTo(2));

        var remainingBooks = Book.GetAllBooks();
        Assert.That(remainingBooks.Any(b => b.ISBN == isbn1), Is.True);
        Assert.That(remainingBooks.Any(b => b.ISBN == isbn2), Is.False);
        Assert.That(remainingBooks.Any(b => b.ISBN == isbn3), Is.True);
    }
}