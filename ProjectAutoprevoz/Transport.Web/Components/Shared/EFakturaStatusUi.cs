using MudBlazor;

namespace Transport.Web.Components.Shared;

// Deljeno između liste e-faktura (Izlazne) i kolone "SEF-Status" na listi
// računa (/fakture) — jedan izvor boje/stila za statusDokumenta čip, da se
// prikaz ne razilazi između te dve stranice.
public static class EFakturaStatusUi
{
    public static Color BojaStatusa(string? status) => status switch
    {
        "Prihvaceno"                => Color.Success,
        "Odbijeno" or "Stornirano"  => Color.Error,
        _                           => Color.Warning
    };
}
