using byt_library.Domain.Entities;

namespace byt_library.Domain.Interfaces;

public interface IDigitalResource : IResource
{
    public int Size { get; set; }
    public string Link { get; set; }

    static readonly List<string> _supportedLists = [ "English", "Polish", "Ukrainian", "Spanish", "Italian" ];
    
    public List<Translation> Translations { get; set; }
    
    public void AddTranslation(string language)
    {
        if (!_supportedLists.Contains(language))
        {
            throw new NotSupportedException($"Language {language} is not supported.");
        }
        Translations.Add(new Translation { Language = language, Link = $"{Link}/{language}" });
    }
    
}