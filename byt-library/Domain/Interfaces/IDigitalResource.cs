namespace byt_library.Domain.Entities;

public interface IDigitalResource
{
    public int Size { get; set; }
    public string Link { get; set; }
    
    public List<Translation> Translations { get; set; }
    
    public void AddTraslation(string language)
    {
        Translations.Add(new Translation { Language = language, Link = $"{Link}/{language}" });
    }
    
}