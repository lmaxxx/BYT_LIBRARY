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
        
        Translations.Add(new Translation { Language = language, Link = $"{Link}/{language}" });
    }
    
}