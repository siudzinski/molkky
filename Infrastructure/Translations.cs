namespace molkky.Infrastructure;

public static class Translations
{
    public static readonly string English = "en";
    public static readonly string Polish = "pl";

    public static readonly Dictionary<string, Dictionary<string, string>> Items = new()
    {
        [English] = new()
        {
            ["NewGameLabel"] = "New game",
            ["EnterPlayerNameLabel"] = "Enter player name",
            ["AddPlayerButton"] = "Add",
            ["StartGameButton"] = "Start game",
            ["NewGameNavLink"] = "New game",
            ["SettingsNavLink"] = "Settings",
            ["RoundLabel"] = "Round",
            ["ScoreLabel"] = "Score",
            ["CancelButton"] = "Cancel",
            ["ConfirmButton"] = "Confirm",
            ["AddScoreLabel"] = "Add score"
        },
        [Polish] = new()
        {
            ["NewGameLabel"] = "Nowa gra",
            ["EnterPlayerNameLabel"] = "Wpisz imię gracza",
            ["AddPlayerButton"] = "Dodaj",
            ["StartGameButton"] = "Rozpocznij grę",
            ["NewGameNavLink"] = "Nowa gra",
            ["SettingsNavLink"] = "Ustawienia",
            ["RoundLabel"] = "Runda",
            ["ScoreLabel"] = "Wynik",
            ["CancelButton"] = "Anuluj",
            ["ConfirmButton"] = "Potwierdź",
            ["AddScoreLabel"] = "Dodać wynik"
        }
    };
}