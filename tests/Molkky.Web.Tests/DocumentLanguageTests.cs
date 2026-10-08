using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Web.Infrastructure;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class DocumentLanguageTests : AppTestContext
{
    private const string SetAttribute = "document.documentElement.setAttribute";

    [Fact]
    public async Task The_page_language_follows_the_apps_language()
    {
        JSInterop.SetupVoid(SetAttribute, "lang", "en").SetVoidResult();
        JSInterop.SetupVoid(SetAttribute, "lang", "pl").SetVoidResult();
        var component = Render<DocumentLanguage>();

        JSInterop.VerifyInvoke(SetAttribute);
        Assert.Equal(["lang", "en"], JSInterop.Invocations[SetAttribute].Single().Arguments);

        await Services.GetRequiredService<Translator>().SetLanguage(Translations.Polish);

        component.WaitForAssertion(() => Assert.Equal(["lang", "pl"], JSInterop.Invocations[SetAttribute].Last().Arguments));
        // Only the language change itself was saved.
        Assert.All(Storage.Writes, key => Assert.Equal(Translator.StorageKey, key));
    }
}
