using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class PlayerColorsTests
{
    // The five colours, in the order players had them before: primary (violet), secondary (pink),
    // info (blue), success (green), warning (orange).
    [Theory]
    [InlineData(0, "bg-violet-600")]
    [InlineData(1, "bg-pink-600")]
    [InlineData(2, "bg-sky-700")]
    [InlineData(3, "bg-green-700")]
    [InlineData(4, "bg-orange-600")]
    public void Each_palette_index_has_its_colour(int index, string avatarBackground)
    {
        Assert.Contains(avatarBackground, PlayerColors.For(index).Avatar.Split(' '));
    }

    [Theory]
    [InlineData(5, "bg-violet-600")]
    [InlineData(6, "bg-pink-600")]
    [InlineData(11, "bg-pink-600")]
    public void The_palette_repeats_after_five_players(int index, string avatarBackground)
    {
        Assert.Contains(avatarBackground, PlayerColors.For(index).Avatar.Split(' '));
        Assert.Equal(PlayerColors.For(index % 5), PlayerColors.For(index));
    }

    [Fact]
    public void The_five_colours_differ_in_every_use()
    {
        var palette = Enumerable.Range(0, 5).Select(PlayerColors.For).ToList();

        Assert.Distinct(palette.Select(color => color.Avatar));
        Assert.Distinct(palette.Select(color => color.Line));
        Assert.Distinct(palette.Select(color => color.Swatch));
    }

    // Every colour is readable on the light and the dark background: white initials, and its own
    // dark-mode shade for the chart.
    [Fact]
    public void Each_colour_has_white_initials_and_a_dark_mode_shade_for_the_chart()
    {
        Assert.All(Enumerable.Range(0, 5).Select(PlayerColors.For), color =>
        {
            Assert.Contains("text-white", color.Avatar.Split(' '));
            Assert.Contains(color.Line.Split(' '), name => name.StartsWith("dark:stroke-", StringComparison.Ordinal));
            Assert.Contains(color.Swatch.Split(' '), name => name.StartsWith("dark:bg-", StringComparison.Ordinal));
        });
    }
}
