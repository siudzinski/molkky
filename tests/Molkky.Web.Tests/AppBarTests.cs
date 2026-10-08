using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Web.Infrastructure;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class AppBarTests : AppTestContext
{
    private Translator Translator => Services.GetRequiredService<Translator>();

    private static IElement Language(IRenderedComponent<AppBar> bar, string name) => bar.Find($"input[type=radio][aria-label={name}]");

    private static IElement MenuButton(IRenderedComponent<AppBar> bar) => bar.Find("button[aria-controls=app-menu]");

    [Fact]
    public void Showing_the_bar_saves_nothing()
    {
        var bar = Render<AppBar>();
        bar.Render();

        Assert.Empty(Storage.Writes);
    }

    [Fact]
    public void Switching_the_language_saves_it_and_translates_the_app()
    {
        var bar = Render<AppBar>();
        Assert.True(Language(bar, "English").HasAttribute("checked"));

        Language(bar, "Polski").Change(true);

        bar.WaitForAssertion(() => Assert.Equal("""{"version":1,"language":"pl"}""", Storage.Items[Translator.StorageKey]));
        Assert.Equal(Translations.Polish, Translator.Language);
        Assert.True(Language(bar, "Polski").HasAttribute("checked"));
        MenuButton(bar).Click();
        Assert.Equal(["Nowa gra", "Ustawienia"], bar.FindAll("nav a").Select(link => link.TextContent.Trim()));
    }

    [Fact]
    public async Task Shows_the_saved_language_as_selected()
    {
        Storage.Items[Translator.StorageKey] = """{"version":1,"language":"pl"}""";
        await Translator.LoadLanguage();

        var bar = Render<AppBar>();

        Assert.True(Language(bar, "Polski").HasAttribute("checked"));
        Assert.False(Language(bar, "English").HasAttribute("checked"));
    }

    [Fact]
    public void The_menu_opens_with_the_pages_and_closes_when_one_is_chosen()
    {
        var bar = Render<AppBar>();
        Assert.Equal("false", MenuButton(bar).GetAttribute("aria-expanded"));
        Assert.Empty(bar.FindAll("nav"));

        MenuButton(bar).Click();

        Assert.Equal("true", MenuButton(bar).GetAttribute("aria-expanded"));
        Assert.Equal([("New game", ""), ("Settings", "settings")], bar.FindAll("nav a").Select(link => (link.TextContent.Trim(), link.GetAttribute("href"))));

        bar.FindAll("nav a")[1].Click();

        Assert.Empty(bar.FindAll("nav"));
        Assert.Equal("false", MenuButton(bar).GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Every_button_has_a_name()
    {
        var bar = Render<AppBar>();
        MenuButton(bar).Click();

        Assert.All(bar.FindAll("button"), button =>
            Assert.False(string.IsNullOrWhiteSpace(button.GetAttribute("aria-label") ?? button.TextContent), button.OuterHtml));
        Assert.Equal("Menu", MenuButton(bar).GetAttribute("aria-label"));
    }
}
