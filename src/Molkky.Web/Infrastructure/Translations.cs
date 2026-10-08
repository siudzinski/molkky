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
    public required string ResumeGameQuestion { get; init; }
    public required string ResumeGameButton { get; init; }
    public required string MissButton { get; init; }
    public required string UndoButton { get; init; }
    public required string LastThrowLabel { get; init; }
    public required string ToWinLabel { get; init; }
    public required string MissesInARowLabel { get; init; }
    public required string PlayerLabel { get; init; }
    public required string ThrowingOrderLabel { get; init; }
    public required string EliminatedLabel { get; init; }
    public required string MenuButton { get; init; }
    public required string LanguageLabel { get; init; }
    // The language's own name, e.g. "Polski": what the language switch calls it in every language.
    public required string LanguageName { get; init; }
    public required string PlayersLabel { get; init; }
    public required string RemovePlayerButton { get; init; }
    public required string MinimumPlayersHint { get; init; }
    public required string SettingsApplyToNewGames { get; init; }
    public required string ScoreChartTitle { get; init; }
    public required string NotFoundTitle { get; init; }
    public required string NotFoundMessage { get; init; }
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
            SettingsLabel = "Settings",
            MaxScoreSettingDescription = "What happens when a player scores more than 50?",
            MaxScoreInHalfOption = "Back to 25",
            MaxScoreBackToZeroOption = "Back to 0",
            MissedThrowsSettingDescription = "What happens when a player throws 3 misses in a row?",
            MissedThrowsDisqualifiedOption = "Disqualified",
            MissedThrowsBackToZeroOption = "Back to 0",
            PlayAgainLabel = "Play again",
            WinnerHeaderText = "won the game",
            EndgameLabel = "Endgame",
            ResumeGameQuestion = "Resume game?",
            ResumeGameButton = "Resume",
            MissButton = "Miss",
            UndoButton = "Undo",
            LastThrowLabel = "Last throw",
            ToWinLabel = "To win",
            MissesInARowLabel = "Misses in a row",
            PlayerLabel = "Player",
            ThrowingOrderLabel = "Throwing order",
            EliminatedLabel = "Out of the game",
            MenuButton = "Menu",
            LanguageLabel = "Language",
            LanguageName = "English",
            PlayersLabel = "Players",
            RemovePlayerButton = "Remove",
            MinimumPlayersHint = "Add at least 2 players to start.",
            SettingsApplyToNewGames = "A new game starts with these settings.",
            ScoreChartTitle = "Score after each round",
            NotFoundTitle = "Not found",
            NotFoundMessage = "Sorry, there's nothing at this address.",
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
            SettingsLabel = "Ustawienia",
            MaxScoreSettingDescription = "Co się stanie, jeśli gracz przekroczy 50 punktów?",
            MaxScoreInHalfOption = "Wraca do 25",
            MaxScoreBackToZeroOption = "Wraca do 0",
            MissedThrowsSettingDescription = "Co się stanie, jeśli gracz spudłuje trzy razy z rzędu?",
            MissedThrowsDisqualifiedOption = "Dyskwalifikacja",
            MissedThrowsBackToZeroOption = "Wraca do 0",
            PlayAgainLabel = "Zagraj ponownie",
            WinnerHeaderText = "wygrał rozgrywkę",
            EndgameLabel = "Podsumowanie",
            ResumeGameQuestion = "Wznowić grę?",
            ResumeGameButton = "Wznów",
            MissButton = "Pudło",
            UndoButton = "Cofnij",
            LastThrowLabel = "Ostatni rzut",
            ToWinLabel = "Do wygranej",
            MissesInARowLabel = "Pudła z rzędu",
            PlayerLabel = "Gracz",
            ThrowingOrderLabel = "Kolejność rzutów",
            EliminatedLabel = "Poza grą",
            MenuButton = "Menu",
            LanguageLabel = "Język",
            LanguageName = "Polski",
            PlayersLabel = "Gracze",
            RemovePlayerButton = "Usuń",
            MinimumPlayersHint = "Dodaj co najmniej 2 graczy, aby zacząć.",
            SettingsApplyToNewGames = "Nowa gra rozpocznie się z tymi ustawieniami.",
            ScoreChartTitle = "Wynik po każdej rundzie",
            NotFoundTitle = "Nie znaleziono",
            NotFoundMessage = "Niestety, pod tym adresem nic nie ma.",
        },
    };

    // The texts in a language, or in English for a language the app does not have.
    public static Texts For(string language) => Languages.GetValueOrDefault(language) ?? Languages[English];
}
