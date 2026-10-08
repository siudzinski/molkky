namespace Molkky.Web.Components;

// Ids and names unique across the page, for labels, aria-labelledby and radio groups. One counter for
// every component: a static field in a generic component would be one per type argument, and two
// SegmentedToggles of different types would then share a radio group.
internal static class ElementIds
{
    private static int _last;

    public static string Next(string prefix) => $"{prefix}-{Interlocked.Increment(ref _last)}";
}
