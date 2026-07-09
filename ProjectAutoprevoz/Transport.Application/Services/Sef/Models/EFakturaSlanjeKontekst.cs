namespace Transport.Application.Services.Sef.Models;

// Podaci koji su potrebni za upis tbl_eInvoice/tbl_lineItem posle uspešnog slanja,
// a nisu deo UBL sadržaja (EFakturaUblInput) — poreklo dokumenta, izabrano PDV
// oslobođenje, korisnik koji šalje, "Model" (poziv na broj format).
public class EFakturaSlanjeKontekst
{
    public int? IdRacuna { get; set; }
    public int? IdClanOslobodjenja { get; set; }
    public string? KeyClanOslobodjenja { get; set; }
    public string? Korisnik { get; set; }
    public string? Model { get; set; }

    // Tip dokumenta sa forme ("FAKTURA"/"AVANSNA FAKTURA"/"DOKUMENT O SMANJENJU"/
    // "DOKUMENT O POVECANJU") — koristi se da se posle uspešnog slanja inkrementira
    // odgovarajući brojač (Broj_Dok_1/2/3) u tbl_Podesavanja.
    public string? TipDokumenta { get; set; }
}
