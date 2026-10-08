using System.Runtime.CompilerServices;
using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

public class TranslationsTests
{
    public static TheoryData<string> Languages => new(Translations.Languages.Keys);

    [Fact]
    public void The_app_is_in_English_and_Polish()
    {
        Assert.Equal(["en", "pl"], Translations.Languages.Keys.Order());
    }

    // A language that leaves a string out does not build; this keeps it that way for new strings.
    [Fact]
    public void Every_string_is_required()
    {
        Assert.All(typeof(Texts).GetProperties(), property =>
        {
            Assert.Equal(typeof(string), property.PropertyType);
            Assert.True(property.IsDefined(typeof(RequiredMemberAttribute), inherit: false), $"{property.Name} is not required");
        });
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void Every_string_has_a_text(string language)
    {
        var texts = Translations.Languages[language];

        Assert.All(typeof(Texts).GetProperties(), property =>
            Assert.False(string.IsNullOrWhiteSpace((string?)property.GetValue(texts)), $"{language}: {property.Name} is blank"));
    }

    [Fact]
    public void An_unknown_language_falls_back_to_Polish()
    {
        Assert.Same(Translations.Languages[Translations.Polish], Translations.For("de"));
        Assert.Same(Translations.Languages[Translations.English], Translations.For("en"));
        Assert.Same(Translations.Languages[Translations.Polish], Translations.For("pl"));
    }
}
