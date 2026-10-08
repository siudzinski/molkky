using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Web.Infrastructure;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class AppBarTests : AppTestContext
{
    private const string ApplyTheme = "molkkyTheme.apply";

    private ThemeStore ThemeStore => Services.GetRequiredService<ThemeStore>();

    private static IElement MenuButton(IRenderedComponent<AppBar> bar) => bar.Find("button[aria-controls=app-menu]");

    private static IElement Button(IRenderedComponent<AppBar> bar, string label) => bar.Find($"button[aria-label='{label}']");

    [Fact]
    public void Showing_the_bar_saves_nothing()
    {
        var bar = Render<AppBar>();
        bar.Render();

        Assert.Empty(Storage.Writes);
    }

    // Both buttons are on the page; the stylesheet shows the one for the other theme (see app.css, dark:).
    [Fact]
    public void The_theme_buttons_are_shown_by_the_theme_on_screen()
    {
        var bar = Render<AppBar>();

        var toDark = Button(bar, "Switch to dark mode").ParentElement!.ClassList;
        var toLight = Button(bar, "Switch to light mode").ParentElement!.ClassList;

        Assert.Contains("dark:hidden", toDark);
        Assert.DoesNotContain("hidden", toDark);
        Assert.Contains("hidden", toLight);
        Assert.Contains("dark:flex", toLight);
    }

    [Theory]
    [InlineData("Switch to dark mode", Theme.Dark, "dark")]
    [InlineData("Switch to light mode", Theme.Light, "light")]
    public void A_theme_button_applies_and_saves_its_theme(string label, Theme theme, string name)
    {
        JSInterop.SetupVoid(ApplyTheme, name).SetVoidResult();
        var bar = Render<AppBar>();

        Button(bar, label).Click();

        bar.WaitForAssertion(() => Assert.Equal($$"""{"version":1,"theme":"{{name}}"}""", Storage.Items[ThemeStore.StorageKey]));
        Assert.Equal(theme, ThemeStore.Theme);
        JSInterop.VerifyInvoke(ApplyTheme);
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
    public async Task The_menu_is_in_the_apps_language()
    {
        var bar = Render<AppBar>();

        await Services.GetRequiredService<Translator>().SetLanguage(Translations.Polish);
        MenuButton(bar).Click();

        bar.WaitForAssertion(() => Assert.Equal(["Nowa gra", "Ustawienia"], bar.FindAll("nav a").Select(link => link.TextContent.Trim())));
        Assert.Equal("Włącz tryb ciemny", bar.FindAll("button")[1].GetAttribute("aria-label"));
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
