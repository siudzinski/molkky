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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // A component that subscribes to more overrides this, and calls it.
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Translator.OnLanguageChanged -= OnLanguageChanged;
        }
    }
}
