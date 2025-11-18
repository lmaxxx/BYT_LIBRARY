using System.Text.Json;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class OnlineMagazine : IDigitalResource
{
    public int Id { get; private set; }
    
    public required bool HasAudio { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Size { get; set; }
    public required string Link { get; set; }
    public required List<Translation> Translations { get; set; }
    public required string PageLink { get; set; }

    private static List<OnlineMagazine> _allOnlineMagazine = new();
    private static int _nextId = 1;
    private static readonly object _lockOnlineMagazine = new();

    public static void AddOnlineMagazine(OnlineMagazine onlineMagazine)
    {
        if (onlineMagazine == null)
            throw new ArgumentNullException(nameof(onlineMagazine), "Cannot add null online magazine to extent");

        lock (_lockOnlineMagazine)
        {
            if (onlineMagazine.Id == 0)
            {
                onlineMagazine.Id = _nextId++;
            }

            if (_allOnlineMagazine.Any(om => om.Id == onlineMagazine.Id))
                throw new InvalidOperationException($"OnlineMaganize with ID {onlineMagazine.Id} already exists in extent");

            _allOnlineMagazine.Add(onlineMagazine);
        }
    }

    public static bool RemoveOnlineMagazine(int id)
    {
        lock (_lockOnlineMagazine)
        {
            var onlineMaganize = _allOnlineMagazine.FirstOrDefault(om => om.Id == id);
            if (onlineMaganize != null)
            {
                return _allOnlineMagazine.Remove(onlineMaganize);
            }
            return false;
        }
    }

    public static OnlineMagazine? GetOnlineMagazineById(int id)
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazine.FirstOrDefault(om => om.Id == id);
        }
    }

    public static IReadOnlyList<OnlineMagazine> GetAllOnlineMagazines()
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazine.AsReadOnly();
        }
    }

    public static int GetOnlineMagazineCount()
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazine.Count;
        }
    }

    public static void ClearOnlineMagazineExtent()
    {
        lock (_lockOnlineMagazine)
        {
            _allOnlineMagazine.Clear();
            _nextId = 1;
        }
    }

    public static void SaveOnlineMagazinesToFile(string filePath)
    {
        lock (_lockOnlineMagazine)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allOnlineMagazine, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadOnlineMagazinesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockOnlineMagazine)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var onlineMagazineList = JsonSerializer.Deserialize<List<OnlineMagazine>>(json, options);

            if (onlineMagazineList != null)
            {
                ClearOnlineMagazineExtent();

                foreach (var onlineMagazine in onlineMagazineList)
                {
                    AddOnlineMagazine(onlineMagazine);
                }
            }
        }
    }
}