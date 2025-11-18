using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Translation
{
    public int Id { get; private set; }
    public string Link { get; set; }
    public string Language { get; set; }

    private static List<Translation> _allTranslations = new();
    private static int _nextId = 1;
    private static readonly object _lockTranslation = new();

    public static void AddTranslation(Translation translation)
    {
        if (translation == null)
            throw new TranslationIsNullException(nameof(translation), "Cannot add null translation to extent");

        lock (_lockTranslation)
        {
            if (translation.Id == 0)
            {
                translation.Id = _nextId++;
            }

            if (_allTranslations.Any(t => t.Id == translation.Id))
                throw new TranslationAlreadyExistsException($"Translation with ID {translation.Id} already exists in extent");

            _allTranslations.Add(translation);
        }
    }

    public static bool RemoveTranslation(int id)
    {
        lock (_lockTranslation)
        {
            var translation = _allTranslations.FirstOrDefault(t => t.Id == id);
            if (translation != null)
            {
                return _allTranslations.Remove(translation);
            }
            return false;
        }
    }

    public static Translation? GetTranslationById(int id)
    {
        lock (_lockTranslation)
        {
            return _allTranslations.FirstOrDefault(t => t.Id == id);
        }
    }

    public static IReadOnlyList<Translation> GetAllTranslations()
    {
        lock (_lockTranslation)
        {
            return _allTranslations.AsReadOnly();
        }
    }

    public static int GetTranslationCount()
    {
        lock (_lockTranslation)
        {
            return _allTranslations.Count;
        }
    }

    public static void ClearTranslationExtent()
    {
        lock (_lockTranslation)
        {
            _allTranslations.Clear();
            _nextId = 1;
        }
    }

    public static void SaveTranslationsToFile(string filePath)
    {
        lock (_lockTranslation)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allTranslations, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadTranslationsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockTranslation)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var translationList = JsonSerializer.Deserialize<List<Translation>>(json, options);

            if (translationList != null)
            {
                ClearTranslationExtent();

                foreach (var translation in translationList)
                {
                    AddTranslation(translation);
                }
            }
        }
    }
}