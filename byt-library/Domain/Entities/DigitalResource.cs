
using System.Text.Json.Serialization;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class DigitalResource : IDigitalResource
{
    public int Size { get; set; }
    public string Link { get; set; }
    
    [JsonInclude]
    private readonly HashSet<Translation> _translations = new();

    [JsonInclude] private readonly Resource _resource;

    [JsonConstructor]
    public DigitalResource(Resource resource, int size, string link)
    {
        Size = size;
        Link = link;
        _resource = resource;
        _resource.AssignDigitalResource(this);
    }

    public void AddTranslation(string language)
    {
        // Ensure there is no other translation with such a language already
        if (_translations.FirstOrDefault(translation =>
                translation.Language.Equals(language, StringComparison.OrdinalIgnoreCase)) != null)
        {
            throw new TranslationAlreadyExistsException(language);
        }
        // Translation automatically adds owner. Adding it to hashset ensures both objects have each other and no other can access this relationship.
        _translations.Add(new Translation($"{Link}/{language}", language, this));
    }

    public bool RemoveTranslation(string language)
    {
        // Remove translation from class extent
        Translation? translation = _translations.FirstOrDefault(translation =>
            translation.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

        if (translation == null) return false;
        
        if (Translation.RemoveTranslation(translation.Link, translation.Language))
        {
            _translations.RemoveWhere(t => t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        return false;
    }
}