namespace Transport.Web.Components.Shared;

// Neutralni model za StavkaUnosDialog — deli ga faktura (Stavka/tbl_artikli_racuna)
// i dokument (StavkaDokumenta/tbl_artikli_dokumenta). Dijalog radi SAMO sa ovim,
// mapiranje na/sa konkretnog entiteta je na pozivaocu (tanak sloj, po jedan
// mapper po strani). Namerno bez: Bon (uvek ogledalo TipRacuna sa glave —
// dijalog ga čita direktno iz TipRacuna parametra), vozilo/idVozila/datum*
// (transport polja, postoje samo na Stavka, dodaje ih mapper na strani fakture),
// brisano (dodaje mapper, čuva postojeću vrednost pri izmeni).
// Cena je JEDNO polje — Stavka i StavkaDokumenta imaju dva stupca (BP/SP) koji
// se oduvek pune identičnom vrednošću (nema razlike nabavna/prodajna cena dok
// ne dođe lager modul) — mapper fan-out-uje u oba.
public class StavkaVM
{
    public int Broj { get; set; }
    public string? IdLager { get; set; }
    public string Barcode { get; set; } = "";
    public string Artikal { get; set; } = "";
    public string JM { get; set; } = "kom";
    public decimal Kolicina { get; set; } = 1;
    public decimal CenaPoJM { get; set; }
    public decimal Rabat { get; set; }
    public decimal CenaPoJMminusRab { get; set; }
    public decimal VrednostMinusRab { get; set; }
    public decimal StopaPDV { get; set; }
    public decimal Osnovica { get; set; }
    public decimal PDV { get; set; }
    public decimal Ukupno { get; set; }
    public decimal Suma { get; set; }
}
