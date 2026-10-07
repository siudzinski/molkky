namespace Molkky.Web.Infrastructure;

// Every visible string. Each language sets all of them: a `required` property left out is a build error.
public sealed class Texts
{
    public required string NewGameLabel { get; init; }
    public required string EnterPlayerNameLabel { get; init; }
    public required string AddPlayerButton { get; init; }
    public required string StartGameButton { get; init; }
    public required string NewGameNavLink { get; init; }
    public required string SettingsNavLink { get; init; }
    public required string RoundLabel { get; init; }
    public required string ScoreLabel { get; init; }
    public required string CancelButton { get; init; }
    public required string ConfirmButton { get; init; }
    public required string AddScoreLabel { get; init; }
    public required string SettingsLabel { get; init; }
    public required string MaxScoreSettingDescription { get; init; }
    public required string MaxScoreInHalfOption { get; init; }
    public required string MaxScoreBackToZeroOption { get; init; }
    public required string MissedThrowsSettingDescription { get; init; }
    public required string MissedThrowsDisqualifiedOption { get; init; }
    public required string MissedThrowsBackToZeroOption { get; init; }
    public required string PlayAgainLabel { get; init; }
    public required string WinnerHeaderText { get; init; }
    public required string EndgameLabel { get; init; }
}

public static class Translations
{
    public const string English = "en";
    public const string Polish = "pl";

    public static IReadOnlyDictionary<string, Texts> Languages { get; } = new Dictionary<string, Texts>
    {
        [English] = new()
        {
            NewGameLabel = "New game",
            EnterPlayerNameLabel = "Enter player name",
            AddPlayerButton = "Add",
            StartGameButton = "Start game",
            NewGameNavLink = "New game",
            SettingsNavLink = "Settings",
            RoundLabel = "Round",
            ScoreLabel = "Score",
            CancelButton = "Cancel",
            ConfirmButton = "Confirm",
            AddScoreLabel = "Add score",
            SettingsLabel = "Settings",
            MaxScoreSettingDescription = "What happens when player scores more than maximum points?",
            MaxScoreInHalfOption = "Max score in half",
            MaxScoreBackToZeroOption = "Back to zero",
            MissedThrowsSettingDescription = "What happens when a player throws 3 misses in a row?",
            MissedThrowsDisqualifiedOption = "Disqualified",
            MissedThrowsBackToZeroOption = "Back to zero",
            PlayAgainLabel = "Play again",
            WinnerHeaderText = "won the game",
            EndgameLabel = "Endgame",
        },
        [Polish] = new()
        {
            NewGameLabel = "Nowa gra",
            EnterPlayerNameLabel = "Wpisz imię gracza",
            AddPlayerButton = "Dodaj",
            StartGameButton = "Rozpocznij grę",
            NewGameNavLink = "Nowa gra",
            SettingsNavLink = "Ustawienia",
            RoundLabel = "Runda",
            ScoreLabel = "Wynik",
            CancelButton = "Anuluj",
            ConfirmButton = "Potwierdź",
            AddScoreLabel = "Dodać wynik",
            SettingsLabel = "Ustawienia",
            MaxScoreSettingDescription = "Co się stanie jeśli gracz przekroczy maksymalną liczbę punktów?",
            MaxScoreInHalfOption = "Maksymalny wynik dzielimy na pół",
            MaxScoreBackToZeroOption = "Wraca do zera",
            MissedThrowsSettingDescription = "Co się stanie jeśli gracz spudłuje trzy razy pod rząd?",
            MissedThrowsDisqualifiedOption = "Dyskwalifikacja",
            MissedThrowsBackToZeroOption = "Wraca do zera",
            PlayAgainLabel = "Zagraj ponownie",
            WinnerHeaderText = "wygrał rozgrywkę",
            EndgameLabel = "Podsumowanie",
        },
    };

    // The texts in a language, or in English for a language the app does not have.
    public static Texts For(string language) => Languages.GetValueOrDefault(language) ?? Languages[English];
}
