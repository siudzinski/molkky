using Bunit;
using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

public class ThemeStoreTests : BunitContext
{
    private const string ApplyTheme = "molkkyTheme.apply";

    private readonly FakeLocalStorage _storage = new();

    private ThemeStore NewStore() => new(_storage, JSInterop.JSRuntime);

    [Theory]
    [InlineData(Theme.System, "system")]
    [InlineData(Theme.Light, "light")]
    [InlineData(Theme.Dark, "dark")]
    public async Task A_theme_is_applied_saved_as_version_1_and_loads_back(Theme theme, string name)
    {
        JSInterop.SetupVoid(ApplyTheme, name).SetVoidResult();

        await NewStore().SetTheme(theme);

        JSInterop.VerifyInvoke(ApplyTheme);
        // index.html reads this document before the app starts: keep the two in step.
        Assert.Equal($$"""{"version":1,"theme":"{{name}}"}""", _storage.Items[ThemeStore.StorageKey]);

        var store = NewStore();
        await store.Load();

        Assert.Equal(theme, store.Theme);
    }

    [Fact]
    public async Task A_theme_change_is_announced()
    {
        JSInterop.SetupVoid(ApplyTheme, "dark").SetVoidResult();
        var store = NewStore();
        var announced = 0;
        store.OnThemeChanged += () => announced++;

        await store.SetTheme(Theme.Dark);

        Assert.Equal(1, announced);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("dark")]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"version":2,"theme":"dark"}""")]
    [InlineData("""{"theme":"dark"}""")]
    [InlineData("""{"version":1,"theme":null}""")]
    [InlineData("""{"version":1,"theme":"Dark"}""")]
    [InlineData("""{"version":1,"theme":"sepia"}""")]
    public async Task Without_a_readable_saved_theme_the_app_follows_the_system(string? json)
    {
        if (json is not null) _storage.Items[ThemeStore.StorageKey] = json;
        var store = NewStore();

        await store.Load();

        Assert.Equal(Theme.System, store.Theme);
    }

    [Fact]
    public async Task Loading_applies_and_saves_nothing()
    {
        _storage.Items[ThemeStore.StorageKey] = """{"version":1,"theme":"dark"}""";

        await NewStore().Load();

        Assert.Empty(_storage.Writes);
        Assert.Empty(JSInterop.Invocations);
    }
}
