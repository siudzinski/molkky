namespace Molkky.Web.Components;

// One line of a LineChart: a value per point on the x axis, from the first. LineClass colours the line
// (a stroke colour class), SwatchClass its legend dot (a background colour class); both whole Tailwind classes.
public sealed record LineSeries(string Name, IReadOnlyList<double> Values, string LineClass, string SwatchClass);
