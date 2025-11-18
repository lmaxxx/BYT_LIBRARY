namespace byt_library.Domain.Entities;

public class OnlineMagazine
{
    public string PageLink { get; set; }

    private static List<OnlineMagazine> _allOnlineMagazines = new();
    private static readonly object _lockOnlineMagazine = new();

    public static void AddOnlineMagazine(OnlineMagazine onlineMagazine)
    {
        if (onlineMagazine == null)
            throw new ArgumentNullException(nameof(onlineMagazine), "Cannot add null online magazine to extent");

        lock (_lockOnlineMagazine)
        {
            if (string.IsNullOrWhiteSpace(onlineMagazine.PageLink))
                throw new ArgumentException("PageLink cannot be empty");

            if (_allOnlineMagazines.Any(om => om.PageLink.Equals(onlineMagazine.PageLink, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"OnlineMagazine with PageLink {onlineMagazine.PageLink} already exists in extent");

            _allOnlineMagazines.Add(onlineMagazine);
        }
    }

    public static bool RemoveOnlineMagazine(string pageLink)
    {
        lock (_lockOnlineMagazine)
        {
            var onlineMagazine = _allOnlineMagazines.FirstOrDefault(om => om.PageLink.Equals(pageLink, StringComparison.OrdinalIgnoreCase));
            if (onlineMagazine != null)
            {
                return _allOnlineMagazines.Remove(onlineMagazine);
            }
            return false;
        }
    }

    public static OnlineMagazine? GetOnlineMagazineByPageLink(string pageLink)
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazines.FirstOrDefault(om => om.PageLink.Equals(pageLink, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<OnlineMagazine> GetAllOnlineMagazines()
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazines.AsReadOnly();
        }
    }

    public static void ClearOnlineMagazineExtent()
    {
        lock (_lockOnlineMagazine)
        {
            _allOnlineMagazines.Clear();
        }
    }
}