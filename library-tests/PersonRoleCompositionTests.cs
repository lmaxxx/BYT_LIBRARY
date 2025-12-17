using byt_library.Domain.Entities;
using byt_library.Domain.Exceptions;

namespace library_tests;

public class PersonRoleCompositionTests
{
    [SetUp]
    public void Setup()
    {
        Person.ClearExtent();
        Student.ClearStudentExtent();
        Staff.ClearStaffExtent();
        Author.ClearAuthorExtent();
    }

    [TearDown]
    public void TearDown()
    {
        Person.ClearExtent();
        Student.ClearStudentExtent();
        Staff.ClearStaffExtent();
        Author.ClearAuthorExtent();
    }

    [Test]
    public void AssignStudent_CreatesValidBidirectionalLink()
    {
        var person = new Person("John", "Doe", new DateTime(1990, 1, 1), "john.doe@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);

        var student = new Student(person, enrollmentDate);

        Assert.That(person.GetStudent(), Is.EqualTo(student), "Person should reference the student");
        Assert.That(student.GetPerson(), Is.EqualTo(person), "Student should reference the person");
        Assert.That(Student.GetAllStudents(), Contains.Item(student), "Student extent should contain the student");
    }

    [Test]
    public void AssignStaff_CreatesValidBidirectionalLink()
    {
        var person = new Person("Jane", "Smith", new DateTime(1992, 5, 15), "jane.smith@test.com");

        var staff = new Staff(person, "IT Department");

        Assert.That(person.GetStaff(), Is.EqualTo(staff), "Person should reference the staff");
        Assert.That(staff.GetPerson(), Is.EqualTo(person), "Staff should reference the person");
        Assert.That(Staff.GetAllStaff(), Contains.Item(staff), "Staff extent should contain the staff");
    }

    [Test]
    public void AssignAuthor_CreatesValidBidirectionalLink()
    {
        var person = new Person("Alice", "Johnson", new DateTime(1988, 3, 20), "alice.johnson@test.com");

        var author = new Author(person, "PenName");

        Assert.That(person.GetAuthor(), Is.EqualTo(author), "Person should reference the author");
        Assert.That(author.GetPerson(), Is.EqualTo(person), "Author should reference the person");
        Assert.That(Author.GetAllAuthors(), Contains.Item(author), "Author extent should contain the author");
    }

    [Test]
    public void AssignStudent_Twice_ThrowsException()
    {
        var person = new Person("Bob", "Wilson", new DateTime(1995, 7, 10), "bob.wilson@test.com");
        var enrollmentDate1 = DateTime.Now.AddDays(-30);
        var enrollmentDate2 = DateTime.Now.AddDays(-15);

        var student1 = new Student(person, enrollmentDate1);

        Assert.Throws<StudentAlreadyExistsException>(() => new Student(person, enrollmentDate2),
            "Should throw exception when assigning Student role twice");
    }

    [Test]
    public void AssignStaff_Twice_ThrowsException()
    {
        var person = new Person("Carol", "Davis", new DateTime(1987, 11, 25), "carol.davis@test.com");

        var staff1 = new Staff(person, "HR Department");

        Assert.Throws<StaffAlreadyExistsException>(() => new Staff(person, "IT Department"),
            "Should throw exception when assigning Staff role twice");
    }

    [Test]
    public void AssignAuthor_Twice_ThrowsException()
    {
        var person = new Person("David", "Brown", new DateTime(1993, 2, 14), "david.brown@test.com");

        var author1 = new Author(person, "FirstPen");

        Assert.Throws<AuthorWithSuchNameAlreadyExistsException>(() => new Author(person, "SecondPen"),
            "Should throw exception when assigning Author role twice");
    }

    [Test]
    public void Person_CanHaveAllThreeRoles_Simultaneously()
    {
        var person = new Person("Emma", "Miller", new DateTime(1991, 6, 30), "emma.miller@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);

        var student = new Student(person, enrollmentDate);
        var staff = new Staff(person, "Teaching");
        var author = new Author(person, "Prof.Pen");

        Assert.That(person.GetStudent(), Is.Not.Null, "Person should have Student role");
        Assert.That(person.GetStaff(), Is.Not.Null, "Person should have Staff role");
        Assert.That(person.GetAuthor(), Is.Not.Null, "Person should have Author role");

        Assert.That(person.GetStudent(), Is.EqualTo(student), "Student role should be correct");
        Assert.That(person.GetStaff(), Is.EqualTo(staff), "Staff role should be correct");
        Assert.That(person.GetAuthor(), Is.EqualTo(author), "Author role should be correct");

        Assert.That(student.GetPerson(), Is.EqualTo(person), "Student back-reference should be correct");
        Assert.That(staff.GetPerson(), Is.EqualTo(person), "Staff back-reference should be correct");
        Assert.That(author.GetPerson(), Is.EqualTo(person), "Author back-reference should be correct");
    }

    [Test]
    public void RemoveOneRole_DoesNotAffectOtherRoles()
    {
        var person = new Person("Frank", "Moore", new DateTime(1989, 9, 5), "frank.moore@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);

        var student = new Student(person, enrollmentDate);
        var staff = new Staff(person, "Research");
        var author = new Author(person, "Researcher");

        Student.RemoveStudent("Frank", "Moore");

        Assert.That(person.GetStudent(), Is.Null, "Student role should be removed");
        Assert.That(person.GetStaff(), Is.Not.Null, "Staff role should remain");
        Assert.That(person.GetAuthor(), Is.Not.Null, "Author role should remain");

        Assert.That(person.GetStaff(), Is.EqualTo(staff), "Staff role should be unchanged");
        Assert.That(person.GetAuthor(), Is.EqualTo(author), "Author role should be unchanged");
    }

    [Test]
    public void RemoveStudent_FromExtent_UpdatesPerson()
    {
        var person = new Person("Grace", "Taylor", new DateTime(1994, 4, 18), "grace.taylor@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);
        var student = new Student(person, enrollmentDate);

        var removed = Student.RemoveStudent("Grace", "Taylor");

        Assert.That(removed, Is.True, "RemoveStudent should return true");
        Assert.That(person.GetStudent(), Is.Null, "Person's student reference should be null");
        Assert.That(Student.GetAllStudents(), Does.Not.Contain(student), "Student extent should not contain the student");
    }

    [Test]
    public void Person_CanBeReassignedRole_AfterRemoval()
    {
        var person = new Person("Henry", "Anderson", new DateTime(1996, 12, 8), "henry.anderson@test.com");
        var enrollmentDate1 = DateTime.Now.AddDays(-60);
        var enrollmentDate2 = DateTime.Now.AddDays(-10);

        var student1 = new Student(person, enrollmentDate1);
        Student.RemoveStudent("Henry", "Anderson");

        var student2 = new Student(person, enrollmentDate2);

        Assert.That(person.GetStudent(), Is.EqualTo(student2), "Person should reference the new student");
        Assert.That(student2.GetPerson(), Is.EqualTo(person), "New student should reference the person");
        Assert.That(Student.GetAllStudents(), Contains.Item(student2), "Student extent should contain the new student");
        Assert.That(Student.GetAllStudents(), Does.Not.Contain(student1), "Student extent should not contain the old student");
    }

    [Test]
    public void RemovePerson_FromExtent_ClearsAllRoles()
    {
        var person = new Person("Iris", "Thomas", new DateTime(1991, 8, 22), "iris.thomas@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);
        var student = new Student(person, enrollmentDate);

        person.RemoveStudent();

        Assert.That(person.GetStudent(), Is.Null, "Person's student reference should be null after removal");
    }

    [Test]
    public void RemovePerson_WithAllRoles_RemovesAllFromExtents()
    {
        var person = new Person("Jack", "Martinez", new DateTime(1985, 10, 12), "jack.martinez@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);

        var student = new Student(person, enrollmentDate);
        var staff = new Staff(person, "Administration");
        var author = new Author(person, "JackM");

        var studentCountBefore = Student.GetAllStudents().Count;
        var staffCountBefore = Staff.GetAllStaff().Count;
        var authorCountBefore = Author.GetAllAuthors().Count;

        Person.RemovePerson("Jack", "Martinez");

        Assert.That(Student.GetAllStudents().Count, Is.EqualTo(studentCountBefore - 1),
            "Student extent count should decrease by 1");
        Assert.That(Staff.GetAllStaff().Count, Is.EqualTo(staffCountBefore - 1),
            "Staff extent count should decrease by 1");
        Assert.That(Author.GetAllAuthors().Count, Is.EqualTo(authorCountBefore - 1),
            "Author extent count should decrease by 1");

        Assert.That(Student.GetAllStudents(), Does.Not.Contain(student),
            "Student extent should not contain the removed student");
        Assert.That(Staff.GetAllStaff(), Does.Not.Contain(staff),
            "Staff extent should not contain the removed staff");
        Assert.That(Author.GetAllAuthors(), Does.Not.Contain(author),
            "Author extent should not contain the removed author");
    }

    [Test]
    public void RemovePerson_LeavesOtherPersonsIntact()
    {
        var person1 = new Person("Kevin", "Garcia", new DateTime(1990, 5, 17), "kevin.garcia@test.com");
        var person2 = new Person("Laura", "Rodriguez", new DateTime(1993, 3, 9), "laura.rodriguez@test.com");

        var enrollmentDate = DateTime.Now.AddDays(-30);
        var student1 = new Student(person1, enrollmentDate);
        var staff1 = new Staff(person1, "Finance");
        var author2 = new Author(person2, "LauraR");

        Person.RemovePerson("Kevin", "Garcia");

        Assert.That(Author.GetAllAuthors(), Contains.Item(author2),
            "Person2's author should still be in extent");
        Assert.That(author2.GetPerson(), Is.EqualTo(person2),
            "Person2's author should still reference person2");
        Assert.That(person2.GetAuthor(), Is.EqualTo(author2),
            "Person2 should still reference their author");
    }

    [Test]
    public void CreateStudent_WithNullPerson_ThrowsException()
    {
        var enrollmentDate = DateTime.Now.AddDays(-30);

        Assert.Throws<PersonIsNullException>(() => new Student(null, enrollmentDate),
            "Should throw PersonIsNullException when person is null");
    }

    [Test]
    public void CreateStudent_WithFutureEnrollmentDate_ThrowsException()
    {
        var person = new Person("Mike", "Lee", new DateTime(1992, 7, 20), "mike.lee@test.com");
        var futureDate = DateTime.Now.AddDays(1);

        Assert.Throws<InvalidEnrollmentDateException>(() => new Student(person, futureDate),
            "Should throw InvalidEnrollmentDateException when enrollment date is in the future");
    }

    [Test]
    public void CreateStaff_WithEmptyDepartment_ThrowsException()
    {
        var person = new Person("Nancy", "White", new DateTime(1988, 11, 3), "nancy.white@test.com");

        Assert.Throws<DepartmentIsEmptyException>(() => new Staff(person, ""),
            "Should throw DepartmentIsEmptyException when department is empty");
        Assert.Throws<DepartmentIsEmptyException>(() => new Staff(person, "   "),
            "Should throw DepartmentIsEmptyException when department is whitespace");
    }

    [Test]
    public void CreateAuthor_WithEmptyNickname_ThrowsException()
    {
        var person = new Person("Oscar", "Harris", new DateTime(1995, 1, 29), "oscar.harris@test.com");

        Assert.Throws<NicknameIsEmptyException>(() => new Author(person, ""),
            "Should throw NicknameIsEmptyException when nickname is empty");
        Assert.Throws<NicknameIsEmptyException>(() => new Author(person, "   "),
            "Should throw NicknameIsEmptyException when nickname is whitespace");
    }

    [Test]
    public void CreateAuthor_WithNullNickname_Succeeds()
    {
        var person = new Person("Patricia", "Clark", new DateTime(1987, 6, 15), "patricia.clark@test.com");

        var author = new Author(person, null);

        Assert.That(author.Nickname, Is.Null, "Author nickname should be null");
        Assert.That(author.GetPerson(), Is.EqualTo(person), "Author should reference the person");
        Assert.That(Author.GetAllAuthors(), Contains.Item(author), "Author extent should contain the author");
    }

    [Test]
    public void AfterRoleCreation_ExtentAndPersonLinksConsistent()
    {
        var person = new Person("Quinn", "Lopez", new DateTime(1994, 9, 11), "quinn.lopez@test.com");
        var enrollmentDate = DateTime.Now.AddDays(-30);

        var student = new Student(person, enrollmentDate);
        var staff = new Staff(person, "Marketing");
        var author = new Author(person, "QuinnL");

        Assert.That(student.GetPerson(), Is.EqualTo(person), "Student should reference person");
        Assert.That(staff.GetPerson(), Is.EqualTo(person), "Staff should reference person");
        Assert.That(author.GetPerson(), Is.EqualTo(person), "Author should reference person");

        Assert.That(person.GetStudent(), Is.EqualTo(student), "Person should reference student");
        Assert.That(person.GetStaff(), Is.EqualTo(staff), "Person should reference staff");
        Assert.That(person.GetAuthor(), Is.EqualTo(author), "Person should reference author");

        Assert.That(Student.GetAllStudents(), Contains.Item(student), "Student extent should contain student");
        Assert.That(Staff.GetAllStaff(), Contains.Item(staff), "Staff extent should contain staff");
        Assert.That(Author.GetAllAuthors(), Contains.Item(author), "Author extent should contain author");
    }

    [Test]
    public void AfterExtentRemoval_PersonReferenceCleared()
    {
        var person = new Person("Rachel", "Hill", new DateTime(1991, 12, 24), "rachel.hill@test.com");
        var staff = new Staff(person, "Sales");

        var removed = Staff.RemoveStaff("Rachel", "Hill");

        Assert.That(removed, Is.True, "RemoveStaff should return true");
        Assert.That(person.GetStaff(), Is.Null, "Person's staff reference should be null");
        Assert.That(Staff.GetAllStaff(), Does.Not.Contain(staff), "Staff extent should not contain the removed staff");
    }
}
