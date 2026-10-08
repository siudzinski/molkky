using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Molkky.Domain;
using Molkky.Web.Pages;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

// Every control on every screen has a name a screen reader can say: a button or link by its text or
// aria-label, a field by its <label> or aria-label. Icon-only buttons are the ones that would miss it.
public class AccessibilityTests : AppTestContext
{
    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    private static string? Name(IElement control, Func<string, string?> labelFor)
    {
        var label = control.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(label)) return label;
        if (control.LocalName != "input") return control.TextContent.Trim();

        var wrapping = control.Closest("label")?.TextContent.Trim();
        return !string.IsNullOrWhiteSpace(wrapping) ? wrapping : labelFor(control.Id ?? "");
    }

    private static void EveryControlHasAName<TComponent>(IRenderedComponent<TComponent> screen)
        where TComponent : IComponent
    {
        string? LabelFor(string id) => screen.FindAll($"label[for='{id}']").SingleOrDefault()?.TextContent.Trim();
        var controls = screen.FindAll("button, a, input");

        Assert.NotEmpty(controls);
        Assert.All(controls, control => Assert.False(string.IsNullOrWhiteSpace(Name(control, LabelFor)), control.OuterHtml));
    }

    [Fact]
    public void New_game()
    {
        var page = Render<NewGame>();
        page.Find("input").Input("Ala");
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add").Click();

        EveryControlHasAName(page);
    }

    [Fact]
    public void Gameplay()
    {
        SaveGame(Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 0, 1, 1, 0, 1, 1, 0));

        EveryControlHasAName(Render<Gameplay>());
    }

    [Fact]
    public void Endgame()
    {
        SaveGame(Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 0, 1, 0, 1, 0));

        EveryControlHasAName(Render<Endgame>());
    }

    [Fact]
    public void Endgame_changing_players()
    {
        SaveGame(Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 0, 1, 0, 1, 0));
        var page = Render<Endgame>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Change players").Click();
        page.Find("[role=dialog] form input").Input("Cyd");
        page.Find("[role=dialog] form").Submit();

        EveryControlHasAName(page);
    }

    [Fact]
    public void Settings() => EveryControlHasAName(Render<Settings>());

    [Fact]
    public void App_bar_with_the_menu_open()
    {
        var bar = Render<AppBar>();
        bar.Find("button[aria-controls=app-menu]").Click();

        EveryControlHasAName(bar);
    }

    [Fact]
    public void Resume_game()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default).Throw(5));

        EveryControlHasAName(Render<ResumeGamePrompt>());
    }

    [Fact]
    public void Not_found() => EveryControlHasAName(Render<NotFound>());
}
