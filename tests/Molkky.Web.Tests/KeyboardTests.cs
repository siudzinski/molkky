using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Molkky.Web.Pages;
using MudBlazor;

namespace Molkky.Web.Tests;

public class KeyboardTests : MudBlazorTestContext
{
    private static IElement Button<TComponent>(IRenderedComponent<TComponent> component, string text)
        where TComponent : IComponent =>
        component.FindAll("button").Single(button => button.TextContent.Trim() == text);

    // In the app the dialog provider sits in MainLayout; tests render it next to the component.
    private (IRenderedComponent<MudDialogProvider> Dialogs, IRenderedComponent<Keyboard> Keyboard) RenderKeyboard(Action<int> onScoreAdded)
    {
        var dialogs = Render<MudDialogProvider>();
        var keyboard = Render<Keyboard>(parameters => parameters.Add(k => k.OnScoreAdded, onScoreAdded));
        return (dialogs, keyboard);
    }

    [Fact]
    public void Shows_a_button_for_every_score_from_0_to_12()
    {
        var (_, keyboard) = RenderKeyboard(_ => { });

        var labels = keyboard.FindAll("button").Select(button => button.TextContent.Trim());

        Assert.Equal(Enumerable.Range(0, 13).Select(score => score.ToString()), labels);
    }

    [Fact]
    public void A_score_is_reported_once_it_is_confirmed()
    {
        int? added = null;
        var (dialogs, keyboard) = RenderKeyboard(score => added = score);

        Button(keyboard, "7").Click();

        dialogs.WaitForAssertion(() => Assert.Contains("Add score 7?", dialogs.Markup));
        Assert.Null(added);

        Button(dialogs, "Confirm").Click();

        keyboard.WaitForAssertion(() => Assert.Equal(7, added));
    }

    [Fact]
    public void A_cancelled_score_is_not_reported()
    {
        int? added = null;
        var (dialogs, keyboard) = RenderKeyboard(score => added = score);

        Button(keyboard, "12").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Add score 12?", dialogs.Markup));

        Button(dialogs, "Cancel").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Add score", dialogs.Markup));
        Assert.Null(added);
    }
}
