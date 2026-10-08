using System.Text.RegularExpressions;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class PlayerColorsTests
{
    private static IEnumerable<PlayerColor> Palette => Enumerable.Range(0, PlayerColors.Count).Select(PlayerColors.For);

    [Fact]
    public void There_are_eight_colours()
    {
        Assert.Equal(8, PlayerColors.Count);
    }

    [Theory]
    [InlineData(0, "bg-player-1")]
    [InlineData(1, "bg-player-2")]
    [InlineData(4, "bg-player-5")]
    [InlineData(7, "bg-player-8")]
    public void Each_palette_index_has_its_colour(int index, string avatarBackground)
    {
        Assert.Contains(avatarBackground, PlayerColors.For(index).Avatar.Split(' '));
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(9, 1)]
    [InlineData(15, 7)]
    public void The_palette_repeats_after_eight_players(int index, int sameAs)
    {
        Assert.Equal(PlayerColors.For(sameAs), PlayerColors.For(index));
    }

    [Fact]
    public void The_colours_differ_in_every_use()
    {
        Assert.Distinct(Palette.Select(color => color.Avatar));
        Assert.Distinct(Palette.Select(color => color.Line));
        Assert.Distinct(Palette.Select(color => color.Swatch));
    }

    // The palette tokens carry the light and the dark value, so the colours follow the theme the app is
    // in, also when it is chosen in the app rather than by the system: no dark: variants.
    [Fact]
    public void Each_colour_is_made_of_palette_tokens()
    {
        Assert.All(Palette, color =>
        {
            Assert.Matches(@"^bg-player-\d text-on-player$", color.Avatar);
            Assert.Matches(@"^stroke-player-\d$", color.Line);
            Assert.Matches(@"^bg-player-\d$", color.Swatch);
        });
    }

    [Fact]
    public void Each_token_has_a_light_and_a_dark_value()
    {
        var css = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "Molkky.Web", "Styles", "app.css"));

        foreach (var token in Enumerable.Range(1, PlayerColors.Count).Select(n => $"--player-{n}").Append("--on-player"))
        {
            Assert.Contains($"--color-{token[2..]}: var({token});", css);
            Assert.Equal(2, Regex.Count(css, $@"^\s*{token}: #[0-9a-f]{{6}};", RegexOptions.Multiline));
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "molkky.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("molkky.slnx not found above the test output.");
    }
}
