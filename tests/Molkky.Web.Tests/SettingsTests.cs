using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class SettingsTests : AppTestContext
{
    private const string ApplyTheme = "molkkyTheme.apply";

    // The game rules' radio buttons, in page order: max score in half, back to zero, disqualified, back to zero.
    private static IReadOnlyList<IElement> RuleRadios(IRenderedComponent<Settings> page) =>
        page.FindAll("section[aria-labelledby=game-rules-heading] input[type=radio]");

    private static IElement Radio(IRenderedComponent<Settings> page, int index) => RuleRadios(page)[index];

    // Selects a segment, as a tap or the arrow keys do.
    private static void Select(IRenderedComponent<Settings> page, int index) => Radio(page, index).Change(true);

    private static bool[] Selected(IRenderedComponent<Settings> page) =>
        RuleRadios(page).Select(radio => radio.HasAttribute("checked")).ToArray();

    // One setting's segments by the setting's name: the segment's name and whether it is selected.
    private static (string Name, bool Selected)[] Segments(IRenderedComponent<Settings> page, string setting) =>
        page.FindAll("fieldset")
            .Single(fieldset => fieldset.QuerySelector("legend")!.TextContent.Trim() == setting)
            .QuerySelectorAll("input[type=radio]")
            .Select(radio => (radio.Closest("label")!.TextContent.Trim(), radio.HasAttribute("checked")))
            .ToArray();

    private static IElement Segment(IRenderedComponent<Settings> page, string name) =>
        page.FindAll("label").Single(label => label.TextContent.Trim() == name).QuerySelector("input[type=radio]")!;

    private GameSettings? SavedSettings() => StorageFormat.LoadSettings(Storage.Items.GetValueOrDefault(SettingsStore.Key));

    [Fact]
    public void Showing_the_settings_saves_nothing()
    {
        var page = Render<Settings>();
        page.Render();

        Assert.Empty(Storage.Writes);
    }

    [Fact]
    public void Each_change_is_saved_straight_away()
    {
        var page = Render<Settings>();

        Select(page, 1);
        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified), SavedSettings()));

        Select(page, 3);
        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero), SavedSettings()));
    }

    [Fact]
    public void A_change_keeps_the_other_saved_setting()
    {
        Storage.Items[SettingsStore.Key] = StorageFormat.SaveSettings(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero));
        var page = Render<Settings>();

        Select(page, 0);

        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero), SavedSettings()));
    }

    [Fact]
    public void Without_saved_settings_the_defaults_are_selected()
    {
        var page = Render<Settings>();

        Assert.Equal([true, false, true, false], Selected(page));
    }

    [Fact]
    public void Shows_the_saved_settings_and_each_change()
    {
        Storage.Items[SettingsStore.Key] = StorageFormat.SaveSettings(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero));
        var page = Render<Settings>();

        Assert.Equal([false, true, false, true], Selected(page));

        Select(page, 2);

        page.WaitForAssertion(() => Assert.Equal([false, true, true, false], Selected(page)));
    }

    // Radio buttons with one name are one group in the browser: one checked among them all.
    [Fact]
    public void Each_setting_is_a_radio_group_of_its_own()
    {
        var page = Render<Settings>();

        var groups = page.FindAll("input[type=radio]").GroupBy(radio => radio.GetAttribute("name")).Select(group => group.Count());

        Assert.Equal([2, 2, 2, 3], groups);
    }

    // Each language by its own name, the default first.
    [Fact]
    public void The_languages_are_listed_by_their_own_names()
    {
        var page = Render<Settings>();

        Assert.Equal([("Polski", false), ("English", true)], Segments(page, "Language"));
    }

    [Fact]
    public void Choosing_a_language_saves_it_and_translates_the_app()
    {
        var page = Render<Settings>();

        Segment(page, "Polski").Change(true);

        page.WaitForAssertion(() => Assert.Equal("""{"version":1,"language":"pl"}""", Storage.Items[Translator.StorageKey]));
        Assert.Equal(Translations.Polish, Services.GetRequiredService<Translator>().Language);
        page.WaitForAssertion(() => Assert.Equal("Ustawienia", page.Find("h1").TextContent));
        Assert.Equal([("Polski", true), ("English", false)], Segments(page, "Język"));
    }

    [Fact]
    public void Without_a_saved_theme_the_system_one_is_selected()
    {
        var page = Render<Settings>();

        Assert.Equal([("System", true), ("Light", false), ("Dark", false)], Segments(page, "Theme"));
    }

    [Theory]
    [InlineData("Light", "light")]
    [InlineData("Dark", "dark")]
    [InlineData("System", "system")]
    public async Task Choosing_a_theme_applies_and_saves_it(string option, string name)
    {
        Storage.Items[ThemeStore.StorageKey] = """{"version":1,"theme":"light"}""";
        await Services.GetRequiredService<ThemeStore>().Load();
        JSInterop.SetupVoid(ApplyTheme, name).SetVoidResult();
        var page = Render<Settings>();

        Segment(page, option).Change(true);

        page.WaitForAssertion(() => Assert.Equal($$"""{"version":1,"theme":"{{name}}"}""", Storage.Items[ThemeStore.StorageKey]));
        JSInterop.VerifyInvoke(ApplyTheme);
        Assert.Contains((option, true), Segments(page, "Theme"));
    }

    // The app bar switches the theme too, while this page is open.
    [Fact]
    public async Task Shows_a_theme_chosen_elsewhere()
    {
        JSInterop.SetupVoid(ApplyTheme, "dark").SetVoidResult();
        var page = Render<Settings>();

        await page.InvokeAsync(() => Services.GetRequiredService<ThemeStore>().SetTheme(Theme.Dark));

        page.WaitForAssertion(() => Assert.Equal([("System", false), ("Light", false), ("Dark", true)], Segments(page, "Theme")));
    }
}
