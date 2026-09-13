using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;

namespace Transport.Infrastructure.Data;

public class TransportDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    public TransportDbContext(DbContextOptions<TransportDbContext> options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Read-only režim (licenca istekla ili SamoCitanje=1) — poslednja brana, ne glavni UX
        // (glavna poruka je traka u MainLayout-u). IZUZETAK: tbl_DefaultValues (stanje panela/
        // filtera) — bez toga korisnik ne može ni panel da zatvori u read-only režimu.
        var imaStvarnihIzmena = ChangeTracker.Entries()
            .Any(e => e.State != EntityState.Unchanged && e.Entity is not DefaultValue);

        if (imaStvarnihIzmena && _currentUser is not null && await _currentUser.JeSamoCitanje())
            throw new InvalidOperationException("Licenca je istekla ili je pristup ograničen na pregled. Upis nije moguć.");

        var userId = _currentUser?.GetIdKorisnika() ?? 0;
        if (userId > 0)
        {
            var now = DateTime.Now;
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditable auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.DatumUnosa  = now;
                        auditable.Izmenio     = userId;
                        auditable.DatumIzmene = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.Izmenio     = userId;
                        auditable.DatumIzmene = now;
                        // Ne diraj DatumUnosa — ostaje originalni datum kreiranja
                        entry.Property(nameof(IAuditable.DatumUnosa)).IsModified = false;
                    }
                }
                else if (entry.Entity is Plata plata && entry.State != EntityState.Unchanged)
                {
                    plata.Izmenio     = userId;
                    plata.DatumIzmene = now;
                }

                if (entry.Entity is KarticaPartnera kartica && entry.State == EntityState.Added)
                {
                    kartica.uneo = userId;
                }
            }
        }
        return await base.SaveChangesAsync(ct);
    }

    // Partneri i finansije
    public DbSet<Partner>      Partneri      { get; set; }
    public DbSet<PartnerRacun> PartnerRacuni { get; set; }
    public DbSet<KarticaPartnera> Kartice    { get; set; }
    public DbSet<KarticaNova>     KarticeNova { get; set; }

    // Centralni log brisanja — ko/kad/forma/opis, samo INSERT, nikad se ne menja/briše
    public DbSet<LogBrisanja> LogoviBrisanja { get; set; }

    // Fakturisanje
    public DbSet<Racun> Racuni { get; set; }
    public DbSet<Stavka> ArtikliRacuna { get; set; }
    public DbSet<GotovinskiRacun> GotovinskiRacuni { get; set; }
    public DbSet<StavkaGotovinskog> StavkeGotovinskog { get; set; }
    public DbSet<Otpremnica> Otpremnice { get; set; }
    public DbSet<StavkaOtpremnice> StavkeOtpremnice { get; set; }
    public DbSet<Ponuda> Ponude { get; set; }
    public DbSet<StavkaPonude> StavkePonude { get; set; }

    // Transport
    public DbSet<NalogPrevoz> NaloziZaPrevoz { get; set; }
    public DbSet<PutniNalogKamion> PutniNalozi { get; set; }
    public DbSet<SacuvaniNalog> SacuvaniNalozi { get; set; }

    // Dozvole MUP
    public DbSet<DozvolaMinistarstva> DozvoleMinistarstva { get; set; }

    // Vozni park i osoblje
    public DbSet<Vozilo>    Vozila     { get; set; }
    public DbSet<VazniDatum> VazniDatumi { get; set; }
    public DbSet<Trosak>    Troskovi   { get; set; }
    public DbSet<Vozac>     Vozaci     { get; set; }
    public DbSet<VozacRacun> VozacRacuni { get; set; }
    public DbSet<Dnevnica> Dnevnice { get; set; }
    public DbSet<Plata> Plate { get; set; }

    // PDV i ePorezi
    public DbSet<VatDeductionRecord> VatDeductionRecords { get; set; }
    public DbSet<ObavestenjePP> ObavestenjaPP { get; set; }
    public DbSet<AnalitikaEpp> AnalitikaEPP { get; set; }
    public DbSet<IndividualVatRecord> IndividualVatRecords { get; set; }
    public DbSet<GroupVatRecord> GroupVatRecords { get; set; }
    public DbSet<EFakturaUlaz> EFaktureUlaz { get; set; }
    public DbSet<EInvoice> EInvoices { get; set; }
    public DbSet<LineItem> LineItems { get; set; }

    // Podešavanja i šifarnici
    public DbSet<DefaultValue>  DefaultValues  { get; set; }
    public DbSet<Sifarnik>      Sifarnici      { get; set; }
    public DbSet<PodaciFirme>   PodaciFirme    { get; set; }
    public DbSet<Banka>         Banke          { get; set; }
    public DbSet<Podesavanja>   Podesavanja    { get; set; }

    // SEF — PDV oslobođenja
    public DbSet<TaxExemption>  TaxExemptions  { get; set; }

    // Podsetnici
    public DbSet<Potsetnik> Podsetnici { get; set; }

    // Role / privilegije
    public DbSet<Role> Role { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================================
        // GLOBAL QUERY FILTERS — Soft Delete za tbl_NalogPrevoz, tbl_putniNalogKamion, tbl_partneri
        // ============================================================================
        modelBuilder.Entity<Partner>()
            .HasQueryFilter(p => p.Brisano == 0 || p.Brisano == null);

        // tbl_partneri ima trigger updatePartnera — EF Core mora koristiti
        // standardni INSERT/UPDATE bez OUTPUT klauzule
        modelBuilder.Entity<Partner>()
            .ToTable("tbl_partneri", t => t.UseSqlOutputClause(false));

        modelBuilder.Entity<NalogPrevoz>()
            .HasQueryFilter(n => n.Brisano == 0 || n.Brisano == null);

        modelBuilder.Entity<PutniNalogKamion>()
            .HasQueryFilter(t => t.Brisano == 0 || t.Brisano == null);

        modelBuilder.Entity<Vozac>()
            .HasQueryFilter(v => v.aktivan == 1 || v.aktivan == null);

        modelBuilder.Entity<VozacRacun>(e =>
        {
            e.ToTable("tbl_vozac_racuni");
            e.HasKey(r => r.idRacuna);
            e.HasQueryFilter(r => r.aktivan == 1);
            e.HasOne(r => r.Vozac)
             .WithMany(v => v.Racuni)
             .HasForeignKey(r => r.idVozaca)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vozilo>()
            .HasQueryFilter(v => v.aktivan == 1);

        // Trosak (tbl_troskovi)
        modelBuilder.Entity<Trosak>(e =>
        {
            e.HasQueryFilter(t => t.brisano == 0);
            e.HasOne(t => t.Vozilo)
             .WithMany()
             .HasForeignKey(t => t.idVozila)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // VazniDatum (tbl_dozvole) — nema aktivan kolonu, nema filter
        modelBuilder.Entity<VazniDatum>(e =>
        {
            e.ToTable("tbl_dozvole");
            e.HasKey(d => d.idDozvole);
            e.HasOne(d => d.Vozilo)
             .WithMany(v => v.VazniDatumi)
             .HasForeignKey(d => d.idVozila)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Banka>()
            .HasQueryFilter(b => b.aktivan == 1 || b.aktivan == null);

        modelBuilder.Entity<DozvolaMinistarstva>()
            .HasQueryFilter(d => d.aktivan == 1);

        // PartnerRacun relacija
        modelBuilder.Entity<PartnerRacun>(e =>
        {
            e.ToTable("tbl_partner_racuni");
            e.HasKey(r => r.idRacuna);
            e.HasOne(r => r.Partner)
             .WithMany(p => p.ZiroRacuni)
             .HasForeignKey(r => r.idPartnera)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================================================
        // PARTNERSHIPS — Relacije između entiteta
        // ============================================================================
        
        // Partner ← Kartice, Racuni, GotovinskiRacuni
        modelBuilder.Entity<KarticaPartnera>()
            .HasKey(k => k.Id);

        // tbl_racuni ima trigger — EF Core mora koristiti standardni UPDATE bez OUTPUT klauzule
        modelBuilder.Entity<Racun>()
            .ToTable("tbl_racuni", t => t.UseSqlOutputClause(false));

        // Računi, kartice i stavke računa (tbl_racuni, tbl_Kartica, tbl_artikli_racuna)
        // prešli sa soft delete na fizičko UPDATE/DELETE + centralni log brisanja
        // (ILogBrisanjaService). Bez query filtera — brisano kolone OSTAJU u bazi
        // (za sad nekorišćene), ne brišu se/koriste se za nove operacije.

        // Partner.Racuni navigacija — bez ovoga EF pravi shadow FK "PartnerBroj"
        // koji ne postoji u tbl_racuni (prava kolona je Id_Partnera)
        modelBuilder.Entity<Partner>()
            .HasMany(p => p.Racuni)
            .WithOne()
            .HasForeignKey(r => r.IdPartnera)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GotovinskiRacun>()
            .HasMany(g => g.Stavke)
            .WithOne(s => s.Racun)
            .HasForeignKey(s => s.BrojRacuna)
            .OnDelete(DeleteBehavior.Restrict);

        // Otpremnica ← Stavke
        modelBuilder.Entity<Otpremnica>()
            .HasMany(o => o.Stavke)
            .WithOne(s => s.Otpremnica)
            .HasForeignKey(s => s.BrojOtpremnice)
            .OnDelete(DeleteBehavior.Restrict);

        // Ponuda ← Stavke
        modelBuilder.Entity<Ponuda>()
            .HasMany(p => p.Stavke)
            .WithOne(s => s.Ponuda)
            .HasForeignKey(s => s.BrojPonude)
            .OnDelete(DeleteBehavior.Restrict);

        // PutniNalogKamion (TURA) ← NalogPrevoz (NALOZI)
        modelBuilder.Entity<PutniNalogKamion>()
            .HasMany(t => t.Nalozi)
            .WithOne(n => n.Tura)
            .HasForeignKey(n => n.IdPutnogNaloga)
            .OnDelete(DeleteBehavior.Restrict);

        // Vozilo ← NaloziZaPrevoz (via IdVozila)
        modelBuilder.Entity<Vozilo>()
            .ToTable("tbl_vozila")
            .HasMany(v => v.Nalozi)
            .WithOne()
            .HasForeignKey(n => n.IdVozila)
            .OnDelete(DeleteBehavior.SetNull);

        // Plate (tbl_plate)
        modelBuilder.Entity<Plata>(e =>
        {
            e.HasQueryFilter(p => p.brisano == 0);
        });

        // ============================================================================
        // KEYLESS ENTITETI — Bez primarnog ključa
        // ============================================================================
        modelBuilder.Entity<AnalitikaEpp>()
            .HasNoKey();

        // ============================================================================
        // SPECIFIČNA MAPIRANJA ZA ANOMALIJE U ŠEMI
        // ============================================================================

        // Podsetnici
        modelBuilder.Entity<Potsetnik>(e =>
        {
            e.ToTable("tbl_Potsetnik");
            e.HasKey(p => p.IdPotsetnik);
            e.Property(p => p.Vrsta).HasMaxLength(100);
            e.Property(p => p.Opis).HasMaxLength(500);
        });

        // ObavestenjePP — mapiranje na tbl_ObavestenjaPP (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<ObavestenjePP>(e =>
        {
            e.ToTable("tbl_ObavestenjaPP");
            e.HasKey(o => o.ObavestenjeID);
            e.Property(o => o.ObavestenjeID).HasColumnName("ObavestenjeID");
            e.Property(o => o.noticeId).HasColumnName("noticeId");
            e.Property(o => o.noticeNumber).HasColumnName("noticeNumber").HasMaxLength(50);
            e.Property(o => o.NoticeDate).HasColumnName("NoticeDate");
            e.Property(o => o.recipientPIB).HasColumnName("recipientPIB").HasMaxLength(50);
            e.Property(o => o.recipientMB).HasColumnName("recipientMB").HasMaxLength(50);
            e.Property(o => o.totalVatAmount).HasColumnName("totalVatAmount").HasColumnType("decimal(18,2)");
            e.Property(o => o.Sender).HasColumnName("Sender").HasMaxLength(500);
            e.Property(o => o.tipSender).HasColumnName("tipSender").HasMaxLength(20);
            e.Property(o => o.statust).HasColumnName("statust").HasMaxLength(50);
            e.Property(o => o.senderId).HasColumnName("senderId");
            e.Property(o => o.documentNumber).HasColumnName("documentNumber").HasMaxLength(50);
        });

        // KarticaNova.IdEfakture — dodata kolona (v211), veza ka tbl_eFakturaUlaz.
        // Ostatak KarticaNova mapiranja je attribute-based na entitetu, ne diramo ga.
        modelBuilder.Entity<KarticaNova>()
            .Property(k => k.IdEfakture)
            .HasColumnName("idEfakture");

        // IndividualVatRecord — mapiranje na tbl_IndividualVatRecord (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<IndividualVatRecord>(e =>
        {
            e.ToTable("tbl_IndividualVatRecord");
            e.HasKey(v => v.idUnosa);
            e.Property(v => v.idUnosa).HasColumnName("idUnosa");
            e.Property(v => v.idIndividualVat).HasColumnName("idIndividualVat");
            e.Property(v => v.year).HasColumnName("year");
            e.Property(v => v.calculationNumber).HasColumnName("calculationNumber").HasMaxLength(500);
            e.Property(v => v.documentNumber).HasColumnName("documentNumber").HasMaxLength(500);
            e.Property(v => v.pibPartnera).HasColumnName("pibPartnera").HasMaxLength(500);
            e.Property(v => v.vatPeriodStr).HasColumnName("vatPeriodStr").HasMaxLength(50);
            e.Property(v => v.documentDirectionStr).HasColumnName("documentDirectionStr").HasMaxLength(50);
            e.Property(v => v.documentType).HasColumnName("documentType").HasMaxLength(50);
            e.Property(v => v.internalInvoiceOption).HasColumnName("internalInvoiceOption");
            e.Property(v => v.relatedPartyIdentifier).HasColumnName("relatedPartyIdentifier").HasMaxLength(500);
            e.Property(v => v.internalInvoiceNumber).HasColumnName("internalInvoiceNumber");
            e.Property(v => v.basisForPrepayment).HasColumnName("basisForPrepayment").HasMaxLength(500);
            e.Property(v => v.recordingDate).HasColumnName("recordingDate");
            e.Property(v => v.statusChangeDate).HasColumnName("statusChangeDate");
            e.Property(v => v.status).HasColumnName("status").HasMaxLength(50);
            e.Property(v => v.totalCalculatedVat).HasColumnName("totalCalculatedVat").HasColumnType("decimal(18,2)");
        });

        // GroupVatRecord — mapiranje na tbl_GroupVatRecord (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<GroupVatRecord>(e =>
        {
            e.ToTable("tbl_GroupVatRecord");
            e.HasKey(v => v.idZbirne);
            e.Property(v => v.idZbirne).HasColumnName("idZbirne");
            e.Property(v => v.idGroupVat).HasColumnName("idGroupVat");
            e.Property(v => v.year).HasColumnName("year");
            e.Property(v => v.calculationNumber).HasColumnName("calculationNumber").HasMaxLength(500);
            e.Property(v => v.documentNumber).HasColumnName("documentNumber").HasMaxLength(500);
            e.Property(v => v.vatPeriodStr).HasColumnName("vatPeriodStr").HasMaxLength(50);
            e.Property(v => v.relatedPartyIdentifier).HasColumnName("relatedPartyIdentifier").HasMaxLength(500);
            e.Property(v => v.recordingDate).HasColumnName("recordingDate");
            e.Property(v => v.statusChangeDate).HasColumnName("statusChangeDate");
            e.Property(v => v.vatRecordingStatus).HasColumnName("vatRecordingStatus").HasMaxLength(50);
            e.Property(v => v.createdUtc).HasColumnName("createdUtc");
        });

        // EFakturaUlaz — mapiranje na tbl_eFakturaUlaz (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<EFakturaUlaz>(e =>
        {
            e.ToTable("tbl_eFakturaUlaz");
            e.HasKey(x => x.idEfakture);
            e.Property(x => x.idEfakture).HasColumnName("idEfakture");
            e.Property(x => x.idRacuna).HasColumnName("idRacuna");
            e.Property(x => x.idPartnera).HasColumnName("idPartnera");
            e.Property(x => x.naziv).HasColumnName("naziv").HasMaxLength(200);
            e.Property(x => x.PIB).HasColumnName("PIB").HasMaxLength(50);
            e.Property(x => x.MB).HasColumnName("MB").HasMaxLength(50);
            e.Property(x => x.tipPrimaoca).HasColumnName("tipPrimaoca").HasMaxLength(20);
            e.Property(x => x.tipFakture).HasColumnName("tipFakture").HasMaxLength(20);
            e.Property(x => x.tipDokumenta).HasColumnName("tipDokumenta").HasMaxLength(50);
            e.Property(x => x.invoiceSentDateUtc).HasColumnName("invoiceSentDateUtc");
            e.Property(x => x.accountingDateUtc).HasColumnName("accountingDateUtc");
            e.Property(x => x.invoiceDateUtc).HasColumnName("invoiceDateUtc");
            e.Property(x => x.paymentDateUtc).HasColumnName("paymentDateUtc");
            e.Property(x => x.Vrednost).HasColumnName("Vrednost").HasColumnType("decimal(18,2)");
            e.Property(x => x.Rabat).HasColumnName("Rabat").HasColumnType("decimal(18,2)");
            e.Property(x => x.Osnovica).HasColumnName("Osnovica").HasColumnType("decimal(18,2)");
            e.Property(x => x.PDV).HasColumnName("PDV").HasColumnType("decimal(18,2)");
            e.Property(x => x.Ukupno).HasColumnName("Ukupno").HasColumnType("decimal(18,2)");
            e.Property(x => x.ugovorBr).HasColumnName("ugovorBr").HasMaxLength(100);
            e.Property(x => x.porudzbinaBr).HasColumnName("porudzbinaBr").HasMaxLength(100);
            e.Property(x => x.tenderBr).HasColumnName("tenderBr").HasMaxLength(100);
            e.Property(x => x.CRFidentifikator).HasColumnName("CRFidentifikator").HasMaxLength(30);
            e.Property(x => x.CRF_Status).HasColumnName("CRF_Status").HasMaxLength(20);
            e.Property(x => x.statusDokumenta).HasColumnName("statusDokumenta").HasMaxLength(20);
            e.Property(x => x.statusDokumentaDobavljaca).HasColumnName("statusDokumentaDobavljaca").HasMaxLength(20);
            e.Property(x => x.statusPlacanja).HasColumnName("statusPlacanja").HasMaxLength(20);
            e.Property(x => x.PDV_dospece).HasColumnName("PDV_dospece").HasMaxLength(30);
            e.Property(x => x.idPoreskoOslobodjenje).HasColumnName("idPoreskoOslobodjenje");
            e.Property(x => x.prilog).HasColumnName("prilog").HasMaxLength(50);
            e.Property(x => x.invoiceID).HasColumnName("invoiceID").HasMaxLength(50);
            e.Property(x => x.salesInvoiceID).HasColumnName("salesInvoiceID").HasMaxLength(50);
            e.Property(x => x.referenceNumber).HasColumnName("referenceNumber").HasMaxLength(50);
            e.Property(x => x.modelNumber).HasColumnName("modelNumber").HasMaxLength(50);
            e.Property(x => x.purchaseInvoiceId).HasColumnName("purchaseInvoiceId").HasMaxLength(50);
            e.Property(x => x.cirID).HasColumnName("cirID").HasMaxLength(50);
            e.Property(x => x.description).HasColumnName("description");
            e.Property(x => x.note).HasColumnName("note");
            e.Property(x => x.cancelInvoiceMessage).HasColumnName("cancelInvoiceMessage");
            e.Property(x => x.acceptRejectMessage).HasColumnName("acceptRejectMessage");
            e.Property(x => x.invoiceFilePath).HasColumnName("invoiceFilePath");
            e.Property(x => x.brojDokumenta).HasColumnName("brojDokumenta").HasMaxLength(50);
            e.Property(x => x.invoiceIDint).HasColumnName("invoiceIDint");
        });

        // EInvoice — mapiranje na tbl_eInvoice (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<EInvoice>(e =>
        {
            e.ToTable("tbl_eInvoice");
            e.HasKey(x => x.idEfakture);
            e.Property(x => x.idEfakture).HasColumnName("idEfakture");
            e.Property(x => x.idRacuna).HasColumnName("idRacuna");
            e.Property(x => x.tipPrimaoca).HasColumnName("tipPrimaoca").HasMaxLength(20);
            e.Property(x => x.tipFakture).HasColumnName("tipFakture").HasMaxLength(20);
            e.Property(x => x.tipDokumenta).HasColumnName("tipDokumenta").HasMaxLength(50);
            e.Property(x => x.brojDokumenta).HasColumnName("brojDokumenta").HasMaxLength(50);
            e.Property(x => x.idPartnera).HasColumnName("idPartnera");
            e.Property(x => x.partner).HasColumnName("partner").HasMaxLength(200);
            e.Property(x => x.pib).HasColumnName("pib").HasMaxLength(20);
            e.Property(x => x.sendInvoiceToCir).HasColumnName("sendInvoiceToCir");
            e.Property(x => x.ugovorBr).HasColumnName("ugovorBr").HasMaxLength(100);
            e.Property(x => x.porudzbinaBr).HasColumnName("porudzbinaBr").HasMaxLength(100);
            e.Property(x => x.tenderBr).HasColumnName("tenderBr").HasMaxLength(100);
            e.Property(x => x.CRFidentifikator).HasColumnName("CRFidentifikator").HasMaxLength(30);
            e.Property(x => x.CRF_Status).HasColumnName("CRF_Status").HasMaxLength(20);
            e.Property(x => x.statusDokumenta).HasColumnName("statusDokumenta").HasMaxLength(20);
            e.Property(x => x.statusPlacanja).HasColumnName("statusPlacanja").HasMaxLength(20);
            e.Property(x => x.PDV_dospece).HasColumnName("PDV_dospece").HasMaxLength(30);
            e.Property(x => x.idPoreskoOslobodjenje).HasColumnName("idPoreskoOslobodjenje");
            e.Property(x => x.clanPoreskogOslobodjenje).HasColumnName("clanPoreskogOslobodjenje").HasMaxLength(30);
            e.Property(x => x.prilog).HasColumnName("prilog").HasMaxLength(50);
            e.Property(x => x.invoiceID).HasColumnName("invoiceID").HasMaxLength(50);
            e.Property(x => x.salesInvoiceID).HasColumnName("salesInvoiceID").HasMaxLength(50);
            e.Property(x => x.purchaseInvoiceId).HasColumnName("purchaseInvoiceId").HasMaxLength(50);
            e.Property(x => x.cirID).HasColumnName("cirID").HasMaxLength(50);
            e.Property(x => x.vremeSlanja).HasColumnName("vremeSlanja");
            e.Property(x => x.kurs).HasColumnName("kurs").HasColumnType("decimal(18,4)");
            e.Property(x => x.valuta).HasColumnName("valuta").HasMaxLength(10);
            e.Property(x => x.avansi).HasColumnName("avansi");
            e.Property(x => x.pratecaDokumenta).HasColumnName("pratecaDokumenta");
            e.Property(x => x.invoiceMessage).HasColumnName("invoiceMessage").HasMaxLength(500);
            e.Property(x => x.acceptRejectMessage).HasColumnName("acceptRejectMessage").HasMaxLength(500);
            e.Property(x => x.cancelInvoiceMessage).HasColumnName("cancelInvoiceMessage");
            e.Property(x => x.prepaymentInvoiceNumber).HasColumnName("prepaymentInvoiceNumber");
            e.Property(x => x.komentar).HasColumnName("komentar").HasMaxLength(1024);
            e.Property(x => x.vatPointDate).HasColumnName("vatPointDate");
            e.Property(x => x.accountingDateUtc).HasColumnName("accountingDateUtc");
            e.Property(x => x.paymentDateUtc).HasColumnName("paymentDateUtc");
            e.Property(x => x.invoiceDateUtc).HasColumnName("invoiceDateUtc");
            e.Property(x => x.invoiceSentDateUtc).HasColumnName("invoiceSentDateUtc");
            e.Property(x => x.totalToPay).HasColumnName("totalToPay").HasColumnType("decimal(18,2)");
            e.Property(x => x.discountPercentage).HasColumnName("discountPercentage").HasColumnType("decimal(18,2)");
            e.Property(x => x.discountAmount).HasColumnName("discountAmount").HasColumnType("decimal(18,2)");
            e.Property(x => x.sumWithoutVat).HasColumnName("sumWithoutVat").HasColumnType("decimal(18,2)");
            e.Property(x => x.vatRate).HasColumnName("vatRate").HasColumnType("decimal(18,2)");
            e.Property(x => x.vatSum).HasColumnName("vatSum").HasColumnType("decimal(18,2)");
            e.Property(x => x.sumWithVat).HasColumnName("sumWithVat").HasColumnType("decimal(18,2)");
            e.Property(x => x.model).HasColumnName("model").HasMaxLength(20);
            e.Property(x => x.pozivNaBroj).HasColumnName("pozivNaBroj").HasMaxLength(50);
            e.Property(x => x.sourceInvoiceSelectionMode).HasColumnName("sourceInvoiceSelectionMode").HasMaxLength(30);
            e.Property(x => x.indebtednessPeriodFromDate).HasColumnName("indebtednessPeriodFromDate");
            e.Property(x => x.indebtednessPeriodToDate).HasColumnName("indebtednessPeriodToDate");
            e.Property(x => x.nijeSaSef).HasColumnName("nijeSaSef");
            e.Property(x => x.sourceInvoices).HasColumnName("sourceInvoices").HasMaxLength(50);
            e.Property(x => x.korisnik).HasColumnName("korisnik").HasMaxLength(50);
            e.Property(x => x.status).HasColumnName("status").HasMaxLength(10);
            e.Property(x => x.invoiceIDint).HasColumnName("invoiceIDint");
        });

        // LineItem — mapiranje na tbl_lineItem (postojeća tabela, kolone se ne menjaju)
        modelBuilder.Entity<LineItem>(e =>
        {
            e.ToTable("tbl_lineItem");
            e.HasKey(x => x.idKolone);
            e.Property(x => x.idKolone).HasColumnName("idKolone");
            e.Property(x => x.idRacuna).HasColumnName("idRacuna");
            e.Property(x => x.rowId).HasColumnName("rowId");
            e.Property(x => x.invoiceId).HasColumnName("invoiceId");
            e.Property(x => x.orderNo).HasColumnName("orderNo");
            e.Property(x => x.code).HasColumnName("code").HasMaxLength(20);
            e.Property(x => x.description).HasColumnName("description").HasMaxLength(2000);
            e.Property(x => x.unit).HasColumnName("unit").HasMaxLength(20);
            e.Property(x => x.unitPrice).HasColumnName("unitPrice").HasColumnType("decimal(18,2)");
            e.Property(x => x.quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
            e.Property(x => x.discountPercentage).HasColumnName("discountPercentage").HasColumnType("decimal(18,2)");
            e.Property(x => x.discountAmount).HasColumnName("discountAmount").HasColumnType("decimal(18,2)");
            e.Property(x => x.sumWithoutVat).HasColumnName("sumWithoutVat").HasColumnType("decimal(18,2)");
            e.Property(x => x.vatRate).HasColumnName("vatRate").HasColumnType("decimal(18,0)");
            e.Property(x => x.vatSum).HasColumnName("vatSum").HasColumnType("decimal(18,2)");
            e.Property(x => x.sumWithVat).HasColumnName("sumWithVat").HasColumnType("decimal(18,2)");
            e.Property(x => x.vatCategoryCode).HasColumnName("vatCategoryCode").HasMaxLength(5);
            e.Property(x => x.tipRacuna).HasColumnName("tipRacuna").HasMaxLength(20);
            e.Property(x => x.cenaSP).HasColumnName("cenaSP").HasColumnType("decimal(18,2)");
            e.Property(x => x.cenaSaRbt).HasColumnName("cenaSaRbt").HasColumnType("decimal(18,2)");
            e.Property(x => x.KeyClan).HasColumnName("KeyClan").HasMaxLength(50);
            e.Property(x => x.idTaxExemption).HasColumnName("idTaxExemption");
        });

        // kursEur zahteva 4 decimale
        modelBuilder.Entity<Podesavanja>()
            .Property(p => p.kursEur)
            .HasPrecision(18, 4);

        // ============================================================================
        // DECIMALNA PRECIZNOST
        // ============================================================================
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("tbl_role");
            e.HasKey(r => r.IdRole);
            e.Property(r => r.IdRole).HasColumnName("idRole");
            e.Property(r => r.Naziv).HasColumnName("naziv").HasMaxLength(50);
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }

        // kursEur na plati zahteva 4 decimale (poklapa se sa tbl_plate.kursEur decimal(18,4))
        modelBuilder.Entity<Plata>()
            .Property(p => p.KursEur)
            .HasPrecision(18, 4);
    }
}
