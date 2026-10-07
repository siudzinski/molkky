using Molkky.Web.Shared;
using MudBlazor;

namespace Molkky.Web.Tests;

public class PlayerColorsTests
{
    // The five colours (and their order) players had before the domain stored a palette index.
    [Theory]
    [InlineData(0, Color.Primary)]
    [InlineData(1, Color.Secondary)]
    [InlineData(2, Color.Info)]
    [InlineData(3, Color.Success)]
    [InlineData(4, Color.Warning)]
    public void Each_palette_index_has_its_colour(int index, Color color)
    {
        Assert.Equal(color, PlayerColors.For(index));
    }

    [Theory]
    [InlineData(5, Color.Primary)]
    [InlineData(6, Color.Secondary)]
    [InlineData(11, Color.Secondary)]
    public void The_palette_repeats_after_five_players(int index, Color color)
    {
        Assert.Equal(color, PlayerColors.For(index));
    }
}
