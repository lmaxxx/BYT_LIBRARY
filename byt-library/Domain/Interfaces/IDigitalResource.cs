using byt_library.Domain.Entities;

namespace byt_library.Domain.Interfaces;

public interface IDigitalResource : IResource
{
    public int Size { get; set; }
    public string Link { get; set; }

    public void AddTranslation(string language);  // Adds new translations (since by language - there is no way to add already existing translation to a digital resource - no shared translations)
    public bool RemoveTranslation(string language);  // Returns true if successfully removed the translation

}