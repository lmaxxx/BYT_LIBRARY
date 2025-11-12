using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Staff : Person
{
    public string Department { get; set; }
    
    private static List<Staff> _allStaff = new();
    private static readonly object _lockStaff = new();

    public Staff(string firstName, string lastName, DateTime dateOfBirth, string department, string? email = null, int id = 0)
        : base(firstName, lastName, dateOfBirth, email, id)
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
            if (_allStaff.Any(s => s.Id == staff.Id))
                throw new InvalidOperationException($"Staff with ID {staff.Id} already exists in Staff extent");

            _allStaff.Add(staff);
        }
    }

    public static bool RemoveStaff(int id)
    {
        lock (_lockStaff)
        {
            var staff = _allStaff.FirstOrDefault(s => s.Id == id);
            if (staff != null)
            {
                _allStaff.Remove(staff);
                RemovePerson(id);
                return true;
            }
            return false;
        }
    }

    public static Staff? GetStaffById(int id)
    {
        lock (_lockStaff)
        {
            return _allStaff.FirstOrDefault(s => s.Id == id);
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

    public static int GetStaffCount()
    {
        lock (_lockStaff)
        {
            return _allStaff.Count;
        }
    }

    public static void ClearStaffExtent()
    {
        lock (_lockStaff)
        {
            foreach (var staff in _allStaff.ToList())
            {
                RemovePerson(staff.Id);
            }
            _allStaff.Clear();
        }
    }

    public static void SaveStaffToFile(string filePath)
    {
        lock (_lockStaff)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allStaff, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadStaffFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockStaff)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var staffList = JsonSerializer.Deserialize<List<Staff>>(json, options);

            if (staffList != null)
            {
                ClearStaffExtent();

                foreach (var staff in staffList)
                {
                    AddStaff(staff);
                }
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Department: {Department}";
    }
}