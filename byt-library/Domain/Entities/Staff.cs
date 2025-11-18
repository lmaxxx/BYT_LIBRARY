namespace byt_library.Domain.Entities;

public class Staff : Person
{
    public string Department { get; set; }
    
    private static List<Staff> _allStaff = new();
    private static readonly object _lockStaff = new();

    public Staff(string firstName, string lastName, string email, DateTime dateOfBirth, string department)
        : base(firstName, lastName, email, dateOfBirth)
    {
        Department = department;
    }

    public static void AddStaff(Staff staff)
    {
        if (staff == null)
            throw new ArgumentNullException(nameof(staff), "Cannot add null staff to extent");

        AddPerson(staff);

        lock (_lockStaff)
        {
            if (_allStaff.Any(s => s.Email.Equals(staff.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Staff with email {staff.Email} already exists in Staff extent");

            _allStaff.Add(staff);
        }
    }

    public static bool RemoveStaff(string email)
    {
        lock (_lockStaff)
        {
            var staff = _allStaff.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (staff != null)
            {
                _allStaff.Remove(staff);
                RemovePerson(email);
                return true;
            }
            return false;
        }
    }

    public static Staff? GetStaffByEmail(string email)
    {
        lock (_lockStaff)
        {
            return _allStaff.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Staff> GetAllStaff()
    {
        lock (_lockStaff)
        {
            return _allStaff.AsReadOnly();
        }
    }

    public static IReadOnlyList<Staff> GetStaffByDepartment(string department)
    {
        lock (_lockStaff)
        {
            return _allStaff.Where(s => s.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                           .ToList()
                           .AsReadOnly();
        }
    }

    public static void ClearStaffExtent()
    {
        lock (_lockStaff)
        {
            foreach (var staff in _allStaff.ToList())
            {
                RemovePerson(staff.Email);
            }
            _allStaff.Clear();
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Department: {Department}";
    }
}