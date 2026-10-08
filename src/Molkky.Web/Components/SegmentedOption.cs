namespace Molkky.Web.Components;

// One segment of a SegmentedToggle. Title, when set, is the segment's name for screen readers and its
// tooltip, e.g. "Polski" for the text "PL".
public sealed record SegmentedOption<TValue>(TValue Value, string Text, string? Title = null);
