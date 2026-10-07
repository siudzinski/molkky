using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

public class TranslationsTests
{
    public static TheoryData<string> Languages => new(Translations.Items.Keys);

    [Theory]
    [MemberData(nameof(Languages))]
    public void Every_language_has_the_same_strings_as_English(string language)
    {
        var english = Translations.Items[Translations.English].Keys.Order();

        Assert.Equal(english, Translations.Items[language].Keys.Order());
        Assert.All(Translations.Items[language].Values, text => Assert.False(string.IsNullOrWhiteSpace(text)));
    }
}
