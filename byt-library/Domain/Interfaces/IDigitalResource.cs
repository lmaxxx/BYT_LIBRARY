using byt_library.Domain.Entities;

namespace byt_library.Domain.Interfaces;

public interface IDigitalResource : IResource
{
    public int Size { get; set; }
    public string Link { get; set; }
    
    public List<Translation> Translations { get; set; }
    
    public void AddTranslation(string language)
    {
        if (!Translation._supportedLanguages.Contains(language))
        {
            throw new NotSupportedException("Language is not supported");
        }

        var translation = Translation.CreateFor(this, language, $"{Link}/{language}");
        Translations.Add(translation);
    }

    public void RemoveTranslation(string language)
    {
        var translation = Translations.FirstOrDefault(t =>
            t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

        if (translation != null)
        {
            Translation.RemoveTranslation(translation.Link, translation.Language);
            Translations.Remove(translation);
        }
    }
    
}