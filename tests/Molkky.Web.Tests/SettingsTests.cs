using AngleSharp.Dom;
using Bunit;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class SettingsTests : MudBlazorTestContext
{
    // The radio buttons under the segments, in page order: max score in half, back to zero, disqualified, back to zero.
    private static IElement Radio(IRenderedComponent<Settings> page, int index) => page.FindAll("input[type=radio]")[index];

    // Selects a segment, as a tap or the arrow keys do.
    private static void Select(IRenderedComponent<Settings> page, int index) => Radio(page, index).Change(true);

    private static bool[] Selected(IRenderedComponent<Settings> page) =>
        page.FindAll("input[type=radio]").Select(radio => radio.HasAttribute("checked")).ToArray();

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

        Assert.Equal([2, 2], groups);
    }
}
