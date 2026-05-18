using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;

namespace Transport.Infrastructure.Data;

/// <summary>
/// Glavni DbContext za Autoprevoz aplikaciju
/// Mapira na postojeću SQL Server bazu "kasa" verzije 7.86
/// </summary>
public class TransportDbContext : DbContext
{
    public TransportDbContext(DbContextOptions<TransportDbContext> options) : base(options)
    {
    }

    // Partneri i finansije
    public DbSet<Partner> Partneri { get; set; }
    public DbSet<KarticaPartnera> Kartice { get; set; }

    // Fakturisanje
    public DbSet<Racun> Racuni { get; set; }
    public DbSet<StavkaRacuna> StavkeRacuna { get; set; }
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

    // Vozni park i osoblje
    public DbSet<Vozilo> Vozila { get; set; }
    public DbSet<Vozac> Vozaci { get; set; }
    public DbSet<Dnevnica> Dnevnice { get; set; }
    public DbSet<Plata> Plate { get; set; }

    // Magacin
    public DbSet<Artikal> Artikli { get; set; }

    // PDV i ePorezi
    public DbSet<VatDeductionRecord> VatDeductionRecords { get; set; }
    public DbSet<ObavestenjePP> ObavestenjaPP { get; set; }
    public DbSet<AnalitikaEpp> AnalitikaEPP { get; set; }

    // Podešavanja
    public DbSet<DefaultValue>  DefaultValues  { get; set; }
    public DbSet<PodaciFirme>   PodaciFirme    { get; set; }
    public DbSet<Banka>         Banke          { get; set; }

    // Podsetnici
    public DbSet<Potsetnik> Podsetnici { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================================
        // GLOBAL QUERY FILTERS — Soft Delete za tbl_NalogPrevoz, tbl_putniNalogKamion, tbl_partneri
        // ============================================================================
        modelBuilder.Entity<Partner>()
            .HasQueryFilter(p => p.Brisano == 0 || p.Brisano == null);

        modelBuilder.Entity<NalogPrevoz>()
            .HasQueryFilter(n => n.Brisano == 0 || n.Brisano == null);

        modelBuilder.Entity<Vozac>()
            .HasQueryFilter(v => v.aktivan == 1 || v.aktivan == null);

        modelBuilder.Entity<Banka>()
            .HasQueryFilter(b => b.aktivan == 1 || b.aktivan == null);

        // ============================================================================
        // PARTNERSHIPS — Relacije između entiteta
        // ============================================================================
        
        // Partner ← Kartice, Racuni, GotovinskiRacuni
        modelBuilder.Entity<KarticaPartnera>()
            .HasKey(k => k.Id);

        modelBuilder.Entity<Racun>()
            .HasMany(r => r.Stavke)
            .WithOne(s => s.Racun)
            .HasForeignKey(s => s.BrojRacuna)
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

        // NalogPrevoz ← PutniNalozi
        modelBuilder.Entity<NalogPrevoz>()
            .HasMany(n => n.PutniNalozi)
            .WithOne(p => p.Nalog)
            .HasForeignKey(p => p.IdNaloga)
            .OnDelete(DeleteBehavior.Restrict);

        // Vozilo ← NaloziZaPrevoz
        modelBuilder.Entity<Vozilo>()
            .HasMany(v => v.Nalozi)
            .WithOne()
            .HasForeignKey(n => n.VoziloId)
            .OnDelete(DeleteBehavior.SetNull);

        // Vozac ← Dnevnice, Plate
        modelBuilder.Entity<Vozac>()
            .HasMany(v => v.Dnevnice)
            .WithOne(d => d.Vozac)
            .HasForeignKey(d => d.VozacId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vozac>()
            .HasMany(v => v.Plate)
            .WithOne(p => p.Vozac)
            .HasForeignKey(p => p.VozacId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // ObavestenjePP — typo "statust" kolona
        modelBuilder.Entity<ObavestenjePP>()
            .Property(o => o.Status)
            .HasColumnName("statust");

        // ============================================================================
        // DECIMALNA PRECIZNOST
        // ============================================================================
        var decimalProperties = new[]
        {
            typeof(Racun).GetProperty(nameof(Racun.BrutoIznos)),
            typeof(Racun).GetProperty(nameof(Racun.PDV)),
            typeof(Racun).GetProperty(nameof(Racun.Ukupno)),
            typeof(Racun).GetProperty(nameof(Racun.Placeno)),
            typeof(Racun).GetProperty(nameof(Racun.Kurs)),
        };

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
    }
}
