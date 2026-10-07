namespace Molkky.Domain.Tests;

public class DomainDependencyTests
{
    [Fact]
    public void The_domain_does_not_reference_Blazor_JS_interop_or_the_UI_library()
    {
        string[] forbidden = ["Microsoft.AspNetCore", "Microsoft.JSInterop", "MudBlazor"];

        var references = typeof(Game).Assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);

        Assert.DoesNotContain(references, name => forbidden.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)));
    }
}
