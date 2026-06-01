using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_Podesavanja")]
public class Podesavanja
{
    [Key]
    public int Broj { get; set; }

    // ── TAB 1 — Opšta podešavanja ──────────────────────────────────────────
    // Novo polje (dodato ALTER-om): nvarchar(100)
    [MaxLength(100)] public string? MestoIzdavanja  { get; set; }

    // Originalno polje: varchar(150)
    [MaxLength(150)] public string? NapomenaPoreska { get; set; }

    // decimal(18,2)
    public decimal? StopaPDV     { get; set; }
    public decimal? nizaStopaPDV { get; set; }

    // decimal(10,0) — generičke decimalne opcije
    public decimal? OpcijaDecimal1 { get; set; }
    public decimal? OpcijaDecimal2 { get; set; }

    // decimal(10,4)
    public decimal? kursEur    { get; set; }

    // decimal(10,0)
    public decimal? ValutaDana { get; set; }

    // ── TAB 2 — Brojevi dokumenata ──────────────────────────────────────────

    // Ture — int u DB
    public int?     Broj_Kalkulacije          { get; set; }
    [MaxLength(50)]  public string? formatBrojaTure           { get; set; }
    public int?     brTureAgencijski          { get; set; }
    [MaxLength(50)]  public string? formatBrojaTureAgencijski { get; set; }

    // int u DB, 0/1 pattern
    public int? koristiOdvojeneBrojeve { get; set; }

    // Nalozi — int u DB
    public int?     Broj_Gotovinskog              { get; set; }
    [MaxLength(50)]  public string? formatBrojaNaloga          { get; set; }
    public int?     Broj_Dok_4                    { get; set; }
    [MaxLength(50)]  public string? formatBrojaNalogaAgencijski { get; set; }

    // Fakturisanje — int u DB
    public int?     Broj_Racuna           { get; set; }
    [MaxLength(50)]  public string? formatBrojaRacuna   { get; set; }
    public int?     OpcijaInt9            { get; set; }
    [MaxLength(50)]  public string? formatBrojaInoRacuna { get; set; }
    public int?     Broj_Predracuna       { get; set; }
    [MaxLength(50)]  public string? formatBrojaPredracuna { get; set; }
    public int?     Broj_Otpremnice       { get; set; }
    [MaxLength(50)]  public string? formatBrojaOtpremnice { get; set; }

    // Slobodni string — OpcijaString12: osnova za valutu (PROMET/RACUN)
    [MaxLength(50)] public string? OpcijaString12 { get; set; }

    // Opšte opcije — int u DB (0/1 pattern za bool, int za cifre)
    public int? automatskiBrojevi       { get; set; }
    public int? rucniUnosBrojFakture    { get; set; }
    public int? koristiOdvojeneInoRacune { get; set; }
    public int? minCifaraBroja          { get; set; }
    [MaxLength(50)] public string? prefiksiRacuna { get; set; }

    // ── TAB 3 — Izgled štampe ───────────────────────────────────────────────
    public string? usloviTransporta  { get; set; }
    public string? napomenaStampa1   { get; set; }
    public string? napomenaStampa2   { get; set; }
    public int?    stampaLogoAktivan { get; set; }

    // Fakturisanje — tekstovi
    public string? Napomena_txt2 { get; set; }
    public string? opcijaText1   { get; set; }
    public string? opcijaText2   { get; set; }

    // ── TAB 4 — E-fakture ───────────────────────────────────────────────────
    [MaxLength(20)]  public string? sefTipServera        { get; set; }
    public string?   sefApiKey            { get; set; }
    [MaxLength(50)]  public string? pdvKategorija        { get; set; }
    [MaxLength(10)]  public string? pdvSlovo             { get; set; }
    [MaxLength(30)]  public string? pdvDatumObracuna     { get; set; }
    public int?      eFakturaAktivna      { get; set; }

    [MaxLength(20)]  public string? eOtpremnicaTipServera { get; set; }
    public string?   eOtpremnicaApiKey    { get; set; }
    public int?      eOtpremnicaAktivna   { get; set; }
}
