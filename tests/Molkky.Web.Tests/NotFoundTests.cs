using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Web.Infrastructure;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class NotFoundTests : MudBlazorTestContext
{
    [Fact]
    public async Task The_not_found_page_is_translated()
    {
        var translator = Services.GetRequiredService<Translator>();
        var page = Render<NotFound>();
        Assert.Contains("Sorry, there's nothing at this address.", page.Markup);

        await translator.SetLanguage(Translations.Polish);

        page.WaitForAssertion(() => Assert.Contains("Niestety, pod tym adresem nic nie ma.", page.Markup));
        Assert.Equal("Nie znaleziono", page.Find("h1").TextContent.Trim());
    }
}
