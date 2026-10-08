using Bunit;
using Molkky.Domain;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class EndgameTests : MudBlazorTestContext
{
    // Ala scores 1 a throw, Bob 12 and Cyd misses: from round 2 on the order is Cyd, Ala, Bob. Cyd is
    // out after round 3; Bob reaches exactly 50 with the last throw of round 5.
    private static Game WonByBob() =>
        new[] { 1, 12, 0, /* round 2 */ 0, 1, 12, /* round 3 */ 0, 1, 12, /* round 4 */ 1, 12, /* round 5 */ 1, 2 }
            .Aggregate(Game.CreateNew(["Ala", "Bob", "Cyd"], new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified)),
                (game, points) => game.Throw(points));

    [Fact]
    public void Shows_the_winner()
    {
        Assert.Equal(["Cyd"], WonByBob().Losers.Select(p => p.Name));
        SaveGame(WonByBob());

        var page = Render<Endgame>();

        Assert.Contains("Bob won the game!", page.Markup);
    }

    [Fact]
    public void Play_again_saves_the_new_game_in_the_finishing_order_and_opens_it()
    {
        var game = WonByBob();
        SaveGame(game);
        var page = Render<Endgame>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Play again").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        var replay = SavedGame()!;
        Assert.Empty(replay.Throws);
        Assert.Equal(game.Settings, replay.Settings);
        Assert.Equal(["Cyd", "Ala", "Bob"], replay.Players.Select(p => p.Name));
    }

    [Fact]
    public void Without_a_finished_game_a_new_game_starts()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));

        Render<Endgame>();

        Assert.Equal("http://localhost/", Uri);
    }
}
