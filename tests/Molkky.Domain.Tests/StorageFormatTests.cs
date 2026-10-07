using Molkky.Domain.Storage;

namespace Molkky.Domain.Tests;

public class StorageFormatTests
{
    private static Game NewGame(MaximumPointsStrategies maximumPoints, MissedThrowsStrategies missedThrows, params string[] names) =>
        Game.CreateNew(names, new GameSettings(maximumPoints, missedThrows));

    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    // Everything the game shows, to compare two games.
    private static string State(Game game) =>
        $"{game.Settings}, round {game.RoundNumber}, {game.CurrentPlayer.Name} to throw, winner {game.Winner?.Name ?? "none"}, " +
        string.Join(" ", game.AllPlayers.Select(p =>
            $"{p.Name}#{p.ColorIndex}:{p.Score}:{p.NumberOfFailedThrows}:{(p.CanPlay ? "in" : "out")}:[{string.Join(",", p.ScoreHistory)}]")) +
        $", throws [{string.Join(",", game.Throws)}]";

    private static Game RoundTrip(Game game) => StorageFormat.LoadGame(StorageFormat.SaveGame(game))!;

    [Theory]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero)]
    public void A_game_loads_back_exactly_after_every_throw(MaximumPointsStrategies maximumPoints, MissedThrowsStrategies missedThrows)
    {
        var random = new Random(3);
        var game = NewGame(maximumPoints, missedThrows, "Ala", "Bob", "Cyd", "Dan", "Ewa", "Fra");

        Assert.Equal(State(game), State(RoundTrip(game)));
        while (!game.AnyWinner)
        {
            game = game.Throw(random.Next(3) == 0 ? 0 : random.Next(1, 13));

            Assert.Equal(State(game), State(RoundTrip(game)));
        }
    }

    [Fact]
    public void A_won_game_loads_back_with_its_winner_and_chart()
    {
        // Ala reaches exactly 50 with the last throw.
        var game = Play(NewGame(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified, "Ala", "Bob"),
            12, 1, 1, 12, 1, 12, 1, 12, 1, 2);

        var loaded = RoundTrip(game);

        Assert.Equal("Ala", loaded.Winner?.Name);
        Assert.Equal(game.ToStats().Rounds, loaded.ToStats().Rounds);
        Assert.Equal(
            game.ToStats().Players.Select(p => (p.Name, string.Join(",", p.ScoreHistory))),
            loaded.ToStats().Players.Select(p => (p.Name, string.Join(",", p.ScoreHistory))));
    }

    [Fact]
    public void A_game_played_again_loads_back_in_the_finishing_order_with_the_same_colours()
    {
        var game = Play(NewGame(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified, "Ala", "Bob", "Cyd"),
            10, 2, 5).PlayAgain();

        Assert.Equal(State(game), State(RoundTrip(game)));
        Assert.Equal([("Bob", 1), ("Cyd", 2), ("Ala", 0)], RoundTrip(game).Players.Select(p => (p.Name, p.ColorIndex)));
    }

    // The stored format, version 1. Changing it means a new version that still loads this one.
    private const string Version1Game = """
        {"version":1,"settings":{"maximumPoints":"BackToZero","missedThrows":"Disqualified"},"players":[{"name":"Ala","color":0},{"name":"Bob","color":1}],"throws":[12,0,5]}
        """;

    [Fact]
    public void A_game_is_saved_as_version_1()
    {
        var game = Play(NewGame(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified, "Ala", "Bob"), 12, 0, 5);

        Assert.Equal(Version1Game, StorageFormat.SaveGame(game));
    }

    [Fact]
    public void A_version_1_game_loads()
    {
        var game = StorageFormat.LoadGame(Version1Game)!;

        Assert.Equal(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified), game.Settings);
        Assert.Equal([12, 0, 5], game.Throws);
        // Round 1: Ala 12, Bob misses. Bob (0) throws first in round 2 and scores 5.
        Assert.Equal([("Bob", 1, 5, 0), ("Ala", 0, 12, 0)], game.AllPlayers.Select(p => (p.Name, p.ColorIndex, p.Score, p.NumberOfFailedThrows)));
        Assert.Equal(2, game.RoundNumber);
        Assert.Equal("Ala", game.CurrentPlayer.Name);
    }

    public static TheoryData<string?> UnreadableGames => new(
        null,
        "",
        "   ",
        "not json",
        "{",
        "null",
        "42",
        "\"game\"",
        "[]",
        "{}",
        // Other versions, or no usable version.
        Version1Game.Replace("\"version\":1", "\"version\":2"),
        Version1Game.Replace("\"version\":1", "\"version\":0"),
        Version1Game.Replace("\"version\":1", "\"version\":\"1\""),
        Version1Game.Replace("\"version\":1", "\"version\":1.5"),
        Version1Game.Replace("\"version\":1,", ""),
        // Version 1 with parts missing or wrong.
        """{"version":1}""",
        Version1Game.Replace(",\"throws\":[12,0,5]", ""),
        Version1Game.Replace("\"throws\":[12,0,5]", "\"throws\":null"),
        Version1Game.Replace("\"throws\":[12,0,5]", "\"throws\":[12,null]"),
        Version1Game.Replace("\"throws\":[12,0,5]", "\"throws\":[\"12\"]"),
        Version1Game.Replace("\"throws\":[12,0,5]", "\"throws\":[13]"),
        Version1Game.Replace("\"throws\":[12,0,5]", "\"throws\":[-1]"),
        Version1Game.Replace("\"settings\":{\"maximumPoints\":\"BackToZero\",\"missedThrows\":\"Disqualified\"},", ""),
        Version1Game.Replace("\"BackToZero\"", "\"Sometimes\""),
        Version1Game.Replace("\"BackToZero\"", "9"),
        Version1Game.Replace(",{\"name\":\"Bob\",\"color\":1}", ""),
        Version1Game.Replace("{\"name\":\"Bob\",\"color\":1}", "null"),
        Version1Game.Replace("\"name\":\"Bob\"", "\"name\":\"\""),
        Version1Game.Replace("\"name\":\"Bob\"", "\"name\":null"),
        Version1Game.Replace(",\"color\":1", ""),
        Version1Game.Replace("\"players\":[{\"name\":\"Ala\",\"color\":0},{\"name\":\"Bob\",\"color\":1}]", "\"players\":null"),
        // Bob is the last player standing after Ala's third miss, so a further throw cannot be part of the game.
        """{"version":1,"settings":{"maximumPoints":"MaxScoreInHalf","missedThrows":"Disqualified"},"players":[{"name":"Ala","color":0},{"name":"Bob","color":1}],"throws":[0,1,0,1,0,5]}""",
        // The phase 2 sessionStorage format.
        """{"Players":[{"Name":"Ala","Score":12,"NumberOfFailedThrows":0,"ScoreHistory":[12],"AvatarColor":"primary"},{"Name":"Bob","Score":0,"NumberOfFailedThrows":0,"ScoreHistory":[],"AvatarColor":"secondary"}],"MaximumPointsStrategy":1,"MissedThrowsStrategy":1,"NumberOfThrowsInRound":1,"RoundNumber":1}""");

    [Theory]
    [MemberData(nameof(UnreadableGames))]
    public void Old_or_unreadable_game_data_loads_as_no_game(string? json)
    {
        Assert.Null(StorageFormat.LoadGame(json));
    }

    [Theory]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero)]
    public void Settings_load_back(MaximumPointsStrategies maximumPoints, MissedThrowsStrategies missedThrows)
    {
        var settings = new GameSettings(maximumPoints, missedThrows);

        Assert.Equal(settings, StorageFormat.LoadSettings(StorageFormat.SaveSettings(settings)));
    }

    private const string Version1Settings = """{"version":1,"maximumPoints":"MaxScoreInHalf","missedThrows":"BackToZero"}""";

    [Fact]
    public void Settings_are_saved_as_version_1()
    {
        var settings = new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero);

        Assert.Equal(Version1Settings, StorageFormat.SaveSettings(settings));
        Assert.Equal(settings, StorageFormat.LoadSettings(Version1Settings));
    }

    public static TheoryData<string?> UnreadableSettings => new(
        null,
        "",
        "not json",
        "[]",
        "{}",
        Version1Settings.Replace("\"version\":1", "\"version\":2"),
        Version1Settings.Replace("\"version\":1,", ""),
        Version1Settings.Replace(",\"missedThrows\":\"BackToZero\"", ""),
        Version1Settings.Replace("\"MaxScoreInHalf\"", "\"Sometimes\""),
        Version1Settings.Replace("\"MaxScoreInHalf\"", "0"),
        Version1Settings.Replace("\"MaxScoreInHalf\"", "null"),
        // The phase 2 sessionStorage format.
        """{"MaximumPointsStrategy":2,"MissedThrowsStrategy":2}""");

    [Theory]
    [MemberData(nameof(UnreadableSettings))]
    public void Old_or_unreadable_settings_load_as_no_settings(string? json)
    {
        Assert.Null(StorageFormat.LoadSettings(json));
    }

    [Fact]
    public void The_default_settings_are_max_score_in_half_and_disqualified()
    {
        Assert.Equal(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified), GameSettings.Default);
    }
}
