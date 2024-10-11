namespace molkky.Domain;

public class ColorProvider
{
    private static readonly List<string> _availableColors = new()
    { 
        "primary", "secondary", "info", "success", "warning"
    };

    private int _currentIndex = 0;

    private ColorProvider() {}

    public static readonly ColorProvider Instance = new();

    public string GetNextColor()
    {
        string color = _availableColors[_currentIndex];
        _currentIndex = (_currentIndex + 1) % _availableColors.Count;

        return color;
    }
}