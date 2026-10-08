using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

public class TranslatorTests
{
    private readonly FakeLocalStorage _storage = new();

    [Fact]
    public async Task A_language_change_is_saved_as_version_1_and_loads_back()
    {
        await new Translator(_storage).SetLanguage(Translations.Polish);

        Assert.Equal("""{"version":1,"language":"pl"}""", _storage.Items[Translator.StorageKey]);

        var translator = new Translator(_storage);
        await translator.LoadLanguage();

        Assert.Equal(Translations.Polish, translator.Language);
        Assert.Equal("Nowa gra", translator.Text.NewGameLabel);
    }

    [Fact]
    public async Task A_language_change_is_announced()
    {
        var translator = new Translator(_storage);
        var announced = 0;
        translator.OnLanguageChanged += () => announced++;

        await translator.SetLanguage(Translations.Polish);

        Assert.Equal(1, announced);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("pl")]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"version":2,"language":"pl"}""")]
    [InlineData("""{"language":"pl"}""")]
    [InlineData("""{"version":1,"language":null}""")]
    [InlineData("""{"version":1,"language":"de"}""")]
    public async Task Without_a_readable_saved_language_the_app_is_in_English(string? json)
    {
        if (json is not null) _storage.Items[Translator.StorageKey] = json;
        var translator = new Translator(_storage);

        await translator.LoadLanguage();

        Assert.Equal(Translations.English, translator.Language);
    }
}
