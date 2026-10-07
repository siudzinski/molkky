using AngleSharp.Dom;
using Bunit;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class SettingsTests : MudBlazorTestContext
{
    // The radio buttons in page order: max score in half, back to zero, disqualified, back to zero.
    private static IElement Radio(IRenderedComponent<Settings> page, int index) => page.FindAll("input[type=radio]")[index];

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

        Radio(page, 1).Click();
        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified), SavedSettings()));

        Radio(page, 3).Click();
        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero), SavedSettings()));
    }

    [Fact]
    public void A_change_keeps_the_other_saved_setting()
    {
        Storage.Items[SettingsStore.Key] = StorageFormat.SaveSettings(new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero));
        var page = Render<Settings>();

        Radio(page, 0).Click();

        page.WaitForAssertion(() => Assert.Equal(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero), SavedSettings()));
    }
}
