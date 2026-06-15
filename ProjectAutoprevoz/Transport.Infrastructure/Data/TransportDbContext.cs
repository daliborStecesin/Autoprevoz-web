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

        modelBuilder.Entity<KarticaPartnera>()
            .HasQueryFilter(k => k.brisano == 0);

        // tbl_racuni ima trigger — EF Core mora koristiti standardni UPDATE bez OUTPUT klauzule
        modelBuilder.Entity<Racun>()
            .ToTable("tbl_racuni", t => t.UseSqlOutputClause(false));

        modelBuilder.Entity<Racun>()
            .HasQueryFilter(r => r.brisano == 0);

        // Stavke racuna (tbl_artikli_racuna) — bez audita, soft delete preko brisano
        modelBuilder.Entity<Stavka>()
            .HasQueryFilter(s => s.brisano == 0);

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

        // ObavestenjePP — typo "statust" kolona
        modelBuilder.Entity<ObavestenjePP>()
            .Property(o => o.Status)
            .HasColumnName("statust");

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
