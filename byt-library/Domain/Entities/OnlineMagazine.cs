using System.Text.Json;

namespace byt_library.Domain.Entities;

public class OnlineMagazine
{
    public int Id { get; private set; }
    public string PageLink { get; set; }

    private static List<OnlineMagazine> _allOnlineMaganizes = new();
    private static int _nextId = 1;
    private static readonly object _lockOnlineMaganize = new();

    public static void AddOnlineMaganize(OnlineMagazine onlineMagazine)
    {
        if (onlineMagazine == null)
            throw new ArgumentNullException(nameof(onlineMagazine), "Cannot add null online magazine to extent");

        lock (_lockOnlineMaganize)
        {
            if (onlineMagazine.Id == 0)
            {
                onlineMagazine.Id = _nextId++;
            }

            if (_allOnlineMaganizes.Any(om => om.Id == onlineMagazine.Id))
                throw new InvalidOperationException($"OnlineMaganize with ID {onlineMagazine.Id} already exists in extent");

            _allOnlineMaganizes.Add(onlineMagazine);
        }
    }

    public static bool RemoveOnlineMaganize(int id)
    {
        lock (_lockOnlineMaganize)
        {
            var onlineMaganize = _allOnlineMaganizes.FirstOrDefault(om => om.Id == id);
            if (onlineMaganize != null)
            {
                return _allOnlineMaganizes.Remove(onlineMaganize);
            }
            return false;
        }
    }

    public static OnlineMagazine? GetOnlineMaganizeById(int id)
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.FirstOrDefault(om => om.Id == id);
        }
    }

    public static IReadOnlyList<OnlineMagazine> GetAllOnlineMaganizes()
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.AsReadOnly();
        }
    }

    public static int GetOnlineMaganizeCount()
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.Count;
        }
    }

    public static void ClearOnlineMaganizeExtent()
    {
        lock (_lockOnlineMaganize)
        {
            _allOnlineMaganizes.Clear();
            _nextId = 1;
        }
    }

    public static void SaveOnlineMaganizesToFile(string filePath)
    {
        lock (_lockOnlineMaganize)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allOnlineMaganizes, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadOnlineMaganizesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockOnlineMaganize)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var onlineMaganizeList = JsonSerializer.Deserialize<List<OnlineMagazine>>(json, options);

            if (onlineMaganizeList != null)
            {
                ClearOnlineMaganizeExtent();

                foreach (var onlineMaganize in onlineMaganizeList)
                {
                    AddOnlineMaganize(onlineMaganize);
                }
            }
        }
    }
}