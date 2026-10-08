using Microsoft.AspNetCore.Components;
using Molkky.Web.Infrastructure;

namespace Molkky.Web.Shared;

public class TranslatableComponentBase : ComponentBase, IDisposable
{
    [Inject]
    public required Translator Translator { get; set; }

    // Every visible string in the current language.
    protected Texts Text => Translator.Text;

    protected override void OnInitialized()
    {
        Translator.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Translator.OnLanguageChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }
}
