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

    // Kategorija sa forme (S10/S20/prazno) — čuva se radi tbl_lineItem.vatCategoryCode,
    // builder i dalje podržava samo S10/S20 (10/20% u PdvProcenat).
    public string PdvKategorija { get; set; } = "";
    public decimal PdvProcenat { get; set; }
    public decimal Pdv { get; set; }
    public decimal Ukupno { get; set; }
}
