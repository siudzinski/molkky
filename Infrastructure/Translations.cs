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
            ["AddScoreLabel"] = "Add score",
            ["SettingsLabel"] = "Settings",
            ["MaxScoreSettingDescription"] = "What happens when player scores more than maximum points?",
            ["MaxScoreSettingOption_MaxScoreInHalf"] = "Max score in half",
            ["MaxScoreSettingOption_BackToZero"] = "Back to zero",
            ["MissedThrowsSettingDescription"] = "What happens when a player throws 3 misses in a row?",
            ["MissedThrowsSettingOption_Disqualified"] = "Disqualified",
            ["MissedThrowsSettingOption_BackToZero"] = "Back to zero"
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
            ["AddScoreLabel"] = "Dodać wynik",
            ["SettingsLabel"] = "Ustawienia",
            ["MaxScoreSettingDescription"] = "Co się stanie jeśli gracz przekroczy maksymalną liczbę punktów?",
            ["MaxScoreSettingOption_MaxScoreInHalf"] = "Maksymalny wynik dzielimy na pół",
            ["MaxScoreSettingOption_BackToZero"] = "Wraca do zera",
            ["MissedThrowsSettingDescription"] = "Co się stanie jeśli gracz spudłuje trzy razy pod rząd?",
            ["MissedThrowsSettingOption_Disqualified"] = "Dyskwalifikacja",
            ["MissedThrowsSettingOption_BackToZero"] = "Wraca do zera"
        }
    };
}