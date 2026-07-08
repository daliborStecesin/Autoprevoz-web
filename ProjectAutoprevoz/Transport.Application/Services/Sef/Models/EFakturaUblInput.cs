namespace Transport.Application.Services.Sef.Models;

public class EFakturaUblInput
{
    // Prodavac — iz Podaci firme (tbl_Podaci)
    public string ProdavacNaziv { get; set; } = "";
    public string ProdavacPib { get; set; } = "";
    public string ProdavacAdresa { get; set; } = "";
    public string ProdavacMesto { get; set; } = "";
    public string ProdavacPostanskiBroj { get; set; } = "";
    public string ProdavacMaticniBroj { get; set; } = "";

    // Kupac — partner sa forme /efakture/unos
    public string KupacNaziv { get; set; } = "";
    public string KupacPib { get; set; } = "";
    public string KupacMaticniBroj { get; set; } = "";
    public string? KupacJbkjs { get; set; }
    public string? KupacAdresa { get; set; }
    public string? KupacMesto { get; set; }
    public string? KupacPostanskiBroj { get; set; }

    // Glava dokumenta
    // Tip dokumenta sa forme — "FAKTURA" (default) ili "AVANSNA FAKTURA" za sad;
    // menja InvoiceTypeCode (380/386), Delivery i InvoicePeriod u builderu.
    public string TipDokumenta { get; set; } = "FAKTURA";
    public string BrojDokumenta { get; set; } = "";
    public DateTime? DatumValute { get; set; }
    public DateTime? DatumPrometa { get; set; }
    public string? InterniBrojZaRutiranje { get; set; }
    public string NastanakPdvObaveze { get; set; } = "Datum prometa";
    public string? BrojNarudzbenice { get; set; }
    public string? BrojTendera { get; set; }
    public string? BrojUgovora { get; set; }
    public string? Vozilo { get; set; }
    public string? BrojCmr { get; set; }
    public string? Komentar { get; set; }
    public string? PozivNaBroj { get; set; }
    public string? ZiroRacunIzdavaoca { get; set; }

    public List<EFakturaUblStavka> Stavke { get; set; } = [];

    // Prateća dokumenta (PDF, max 3) — Base64 već gotov u memoriji pre slanja,
    // builder samo ugrađuje, bez ikakve konverzije u ovom trenutku.
    public List<EFakturaUblPrilog> Prilozi { get; set; } = [];

    // Slovo -> KeyClan izabranog člana oslobođenja (sa forme, do 2 stavke).
    // Builder ovo koristi za TaxExemptionReasonCode po grupi u TaxTotal-u.
    public List<EFakturaUblOslobodjenje> Oslobodjenja { get; set; } = [];

    // Avansi izabrani na formi (odbici od ove fakture) — builder emituje
    // SrbDtExt/BillingReference/PrepaidAmount SAMO ako bar jedan avans ima
    // bar jednu kategoriju sa iskorišćenom osnovicom > 0.
    public List<EFakturaUblIzabraniAvans> IzabraniAvansi { get; set; } = [];
}

public class EFakturaUblIzabraniAvans
{
    public string BrojDokumenta { get; set; } = "";
    public DateTime? DatumIzdavanja { get; set; }
    public List<EFakturaUblAvansKategorija> Kategorije { get; set; } = [];
}

public class EFakturaUblAvansKategorija
{
    // Ista konvencija kao StavkaVM/PdvKategorija: "S20"/"S10" za PDV stope,
    // slovo (Z/O/OE/E/AE10/AE20/SS/R/N) za oslobođene kategorije.
    public string Slovo { get; set; } = "";
    public decimal Stopa { get; set; }
    public decimal IskorisenaOsnovica { get; set; }
    public decimal IskorisenPdv { get; set; }
}

public class EFakturaUblPrilog
{
    public string ImeFajla { get; set; } = "";
    public string Base64 { get; set; } = "";
}

public class EFakturaUblOslobodjenje
{
    public string Slovo { get; set; } = "";
    public string KeyClan { get; set; } = "";
}

public class EFakturaUblStavka
{
    public string Sifra { get; set; } = "";
    public string Naziv { get; set; } = "";
    public string Jm { get; set; } = "kom";
    public decimal Kolicina { get; set; }
    public decimal Cena { get; set; }
    public decimal Umanjenje { get; set; }
    public decimal Osnovica { get; set; }

    // Kategorija IZ MODELA (S20/S10 za PDV stavke; slovo člana za oslobođene) —
    // builder je čita, ne preračunava. Builder izvodi TaxCategory ID/Percent iz nje.
    public string PdvKategorija { get; set; } = "";
    public decimal PdvProcenat { get; set; }
    public decimal Pdv { get; set; }
    public decimal Ukupno { get; set; }
}
