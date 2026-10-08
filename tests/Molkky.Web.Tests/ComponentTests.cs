using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Molkky.Web.Components;

namespace Molkky.Web.Tests;

// The generic components in Components/: what they do beyond markup.
public class ComponentTests : AppTestContext
{
    [Fact]
    public void An_icon_button_is_named_by_its_label()
    {
        var clicks = 0;
        var button = Render<IconButton>(parameters => parameters
            .Add(b => b.Icon, IconName.Undo)
            .Add(b => b.Label, "Undo")
            .Add(b => b.OnClick, () => clicks++));

        var element = button.Find("button");
        element.Click();

        Assert.Equal("Undo", element.GetAttribute("aria-label"));
        Assert.Equal("Undo", element.GetAttribute("title"));
        Assert.Equal("true", button.Find("svg").GetAttribute("aria-hidden"));
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void A_disabled_button_is_disabled()
    {
        var button = Render<Button>(parameters => parameters.Add(b => b.Disabled, true).AddChildContent("Start game"));

        Assert.True(button.Find("button").HasAttribute("disabled"));
        Assert.Equal("button", button.Find("button").GetAttribute("type"));
    }

    [Fact]
    public void A_segmented_toggle_shows_its_value_and_reports_a_new_one()
    {
        string? chosen = null;
        var toggle = Render<SegmentedToggle<string>>(parameters => parameters
            .Add(t => t.Label, "Language")
            .Add(t => t.Options, [new("en", "EN", "English"), new("pl", "PL", "Polski")])
            .Add(t => t.Value, "en")
            .Add(t => t.ValueChanged, value => chosen = value));

        Assert.Equal("Language", toggle.Find("legend").TextContent.Trim());
        Assert.Equal([true, false], toggle.FindAll("input[type=radio]").Select(radio => radio.HasAttribute("checked")));
        Assert.Equal(["English", "Polski"], toggle.FindAll("input[type=radio]").Select(radio => radio.GetAttribute("aria-label")));
        Assert.Single(toggle.FindAll("input[type=radio]").Select(radio => radio.GetAttribute("name")).Distinct());

        toggle.FindAll("input[type=radio]")[1].Change(true);

        Assert.Equal("pl", chosen);
        Assert.Equal([false, true], toggle.FindAll("input[type=radio]").Select(radio => radio.HasAttribute("checked")));
    }

    [Fact]
    public void A_text_field_is_labelled_and_reports_each_keystroke()
    {
        var typed = new List<string>();
        var field = Render<TextField>(parameters => parameters
            .Add(f => f.Label, "Enter player name")
            .Add(f => f.ValueChanged, value => typed.Add(value)));

        var input = field.Find("input");
        input.Input("A");
        field.Find("input").Input("Al");

        Assert.Equal(input.Id, field.Find("label").GetAttribute("for"));
        Assert.Equal("Enter player name", field.Find("label").TextContent.Trim());
        Assert.Equal(["A", "Al"], typed);
    }

    [Fact]
    public void A_sheet_is_a_modal_dialog_named_by_its_title_and_takes_the_focus()
    {
        var sheet = Render<Sheet>(parameters => parameters
            .Add(s => s.Title, "Resume game?")
            .Add(s => s.Actions, (RenderFragment)(builder => builder.AddMarkupContent(0, "<button>Resume</button>"))));

        var dialog = sheet.Find("[role=dialog]");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("Resume game?", sheet.Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent.Trim());
        JSInterop.VerifyFocusAsyncInvoke();
    }

    [Fact]
    public void A_line_chart_puts_0_at_the_bottom_Max_at_the_top_and_spreads_the_points_evenly()
    {
        var chart = RenderChart(new LineSeries("Bob", [0, 25, 50], "stroke-pink-600", "bg-pink-600"));

        var points = chart.Find("polyline").GetAttribute("points")!.Split(' ').Select(point => point.Split(',').Select(number => double.Parse(number, CultureInfo.InvariantCulture)).ToArray()).ToArray();

        const double bottom = LineChart.Height - LineChart.Bottom;
        Assert.Equal([[LineChart.Left, bottom], [(LineChart.Left + LineChart.Width - LineChart.Right) / 2.0, (bottom + LineChart.Top) / 2], [LineChart.Width - LineChart.Right, LineChart.Top]], points);
    }

    // SVG wants "12.5" in every language; Polish would format it as "12,5".
    [Fact]
    public void A_line_chart_writes_its_numbers_the_same_in_every_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
        try
        {
            var chart = RenderChart(new LineSeries("Bob", [0, 7, 12.5], "stroke-pink-600", "bg-pink-600"));

            // Each point is "x,y": one comma, between the coordinates.
            Assert.All(chart.Find("polyline").GetAttribute("points")!.Split(' '), point => Assert.Equal(2, point.Split(',').Length));
            Assert.Equal(["0", "7", "12.5"], chart.FindAll("table td").Select(cell => cell.TextContent.Trim()));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void A_line_chart_has_an_accessible_name_and_a_table_of_its_numbers()
    {
        var chart = RenderChart(new LineSeries("Bob", [0, 12], "stroke-pink-600", "bg-pink-600"), new LineSeries("Ala", [0], "stroke-violet-600", "bg-violet-600"));

        var svg = chart.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Score after each round", chart.Find($"#{svg.GetAttribute("aria-labelledby")}").TextContent.Trim());
        Assert.Equal(["Player", "Round 0", "Round 1"], chart.FindAll("table thead th").Select(cell => cell.TextContent.Trim()));
        Assert.Equal([["Bob", "0", "12"], ["Ala", "0", ""]], chart.FindAll("table tbody tr").Select(row => row.Children.Select(cell => cell.TextContent.Trim()).ToArray()));
    }

    private IRenderedComponent<LineChart> RenderChart(params LineSeries[] series) =>
        Render<LineChart>(parameters => parameters
            .Add(c => c.Title, "Score after each round")
            .Add(c => c.Series, series)
            .Add(c => c.Max, 50)
            .Add(c => c.XLabel, "Round")
            .Add(c => c.SeriesLabel, "Player"));
}
