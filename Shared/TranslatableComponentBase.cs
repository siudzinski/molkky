using Microsoft.AspNetCore.Components;
using molkky.Infrastructure;

namespace molkky.Shared;

public class TranslatableComponentBase : ComponentBase, IDisposable
{
    [Inject] 
    public required Translator Translator { get; set; }

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
