using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class ScorePadTests : AppTestContext
{
    private readonly List<int> _thrown = [];

    private IRenderedComponent<ScorePad> RenderPad() =>
        Render<ScorePad>(parameters => parameters.Add(pad => pad.OnThrow, points => _thrown.Add(points)));

    private static string Label(IElement button) => button.TextContent.Trim();

    private static IElement Button(IRenderedComponent<ScorePad> pad, string label) =>
        pad.FindAll("button").Single(button => Label(button) == label);

    [Fact]
    public void Every_button_throws_a_different_score_and_together_they_are_0_to_12()
    {
        var pad = RenderPad();
        var count = pad.FindAll("button").Count;

        // A tap re-renders the pad: find the buttons again each time.
        for (var i = 0; i < count; i++)
        {
            pad.FindAll("button")[i].Click();
        }

        Assert.Equal(13, count);
        Assert.Equal(Enumerable.Range(0, 13), _thrown.Order());
    }

    // As on the field: 7 9 8 / 5 11 12 6 / 3 10 4 / 1 2, then Miss.
    [Fact]
    public void The_pins_stand_in_their_formation_followed_by_miss()
    {
        var pad = RenderPad();

        Assert.Equal(["7", "9", "8", "5", "11", "12", "6", "3", "10", "4", "1", "2", "Miss"], pad.FindAll("button").Select(Label));

        // Rows and columns of the pins' grid: each pin is two columns wide, every other row shifted by one.
        var rows = pad.FindAll("button[class*=row-start-]")
            .GroupBy(pin => Regex.Match(pin.ClassName!, @"\brow-start-(\d)\b").Groups[1].Value)
            .Select(row => row.Select(pin => (Label(pin), Regex.Match(pin.ClassName!, @"\bcol-start-(\d)\b").Groups[1].Value)));
        Assert.Equal(
            [
                [("7", "2"), ("9", "4"), ("8", "6")],
                [("5", "1"), ("11", "3"), ("12", "5"), ("6", "7")],
                [("3", "2"), ("10", "4"), ("4", "6")],
                [("1", "3"), ("2", "5")],
            ],
            rows);
    }

    [Fact]
    public void A_tap_on_a_pin_throws_its_score_once_straight_away()
    {
        var pad = RenderPad();

        Button(pad, "7").Click();

        Assert.Equal([7], _thrown);
    }

    [Fact]
    public void Miss_throws_0()
    {
        var pad = RenderPad();

        Button(pad, "Miss").Click();

        Assert.Equal([0], _thrown);
    }

    [Fact]
    public void Nothing_is_thrown_until_a_button_is_tapped()
    {
        var pad = RenderPad();
        pad.Render();

        Assert.Empty(_thrown);
    }

    [Fact]
    public void Each_tap_is_a_throw_of_its_own()
    {
        var pad = RenderPad();

        Button(pad, "12").Click();
        Button(pad, "12").Click();
        Button(pad, "Miss").Click();

        Assert.Equal([12, 12, 0], _thrown);
    }
}
