namespace Molkky.Web.Components;

// One line of a LineChart: a value per point on the x axis, from the first. LineClass colours the line
// (e.g. "stroke-violet-600"), SwatchClass its legend dot (e.g. "bg-violet-600"); both whole Tailwind classes.
public sealed record LineSeries(string Name, IReadOnlyList<double> Values, string LineClass, string SwatchClass);
