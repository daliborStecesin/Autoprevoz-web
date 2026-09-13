using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;

namespace Transport.Infrastructure.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Licenca>     Licence      { get; set; }
    public DbSet<WebKorisnik> WebKorisnici { get; set; }
    public DbSet<WebLicenca>  WebLicence   { get; set; }
    public DbSet<WebClanstvo> WebClanstva  { get; set; }
    public DbSet<WebRola>     WebRole      { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Licenca>(e =>
        {
            e.ToTable("tbl_licence");
            e.HasKey(l => l.IdLicence);
            e.Property(l => l.Naziv).HasMaxLength(200);
            e.Property(l => l.PIB).HasColumnName("pib").HasMaxLength(50);
            e.Property(l => l.ConnectionString).HasMaxLength(500);
            e.Property(l => l.PorukaKupcu).HasMaxLength(500);
            e.Property(l => l.BrojLicenci).HasColumnName("brojLicenci");
            e.Property(l => l.Program).HasColumnName("program").HasMaxLength(50);
            e.Property(l => l.TipLicence).HasColumnName("tipLicence").HasMaxLength(50);
            e.Property(l => l.Datum).HasColumnName("datum");
        });

        mb.Entity<WebKorisnik>(e =>
        {
            e.ToTable("tbl_web_korisnici");
            e.HasKey(w => w.IdKorisnika);
            e.Property(w => w.Ime).HasMaxLength(100);
            e.Property(w => w.Email).HasMaxLength(200);
            e.Property(w => w.LozinkaHash).HasMaxLength(500);
        });

        mb.Entity<WebLicenca>(e =>
        {
            e.ToTable("tbl_web_licence");
            e.HasKey(l => l.IdWebLicence);
            e.Property(l => l.Naziv).HasMaxLength(200).IsRequired();
            e.Property(l => l.Pib).HasMaxLength(20).IsRequired();
            e.Property(l => l.MaticniBroj).HasMaxLength(20);
            e.Property(l => l.Adresa).HasMaxLength(200);
            e.Property(l => l.Mesto).HasMaxLength(100);
            e.Property(l => l.PostanskiBroj).HasMaxLength(20);
            e.Property(l => l.Zemlja).HasMaxLength(50).IsRequired();
            e.Property(l => l.KodDrzave).HasMaxLength(5).IsRequired();
            e.Property(l => l.ImeBaze).HasMaxLength(128);
            e.Property(l => l.ConnectionString).HasMaxLength(500);
            e.Property(l => l.TipPrograma).HasMaxLength(20).IsRequired();
            e.Property(l => l.TipLicence).HasMaxLength(20).IsRequired();
            e.Property(l => l.DatumOd).HasColumnType("date");
            e.Property(l => l.DatumDo).HasColumnType("date");
            e.Property(l => l.Telefon).HasMaxLength(50);
            e.Property(l => l.Email).HasMaxLength(200);
            e.Property(l => l.IzvorPrijave).HasMaxLength(30);
            e.Property(l => l.Napomena).HasMaxLength(1000);
        });

        mb.Entity<WebRola>(e =>
        {
            e.ToTable("tbl_web_role");
            e.HasKey(r => r.IdWebRole);
            e.Property(r => r.IdWebRole).ValueGeneratedNever();
            e.Property(r => r.Naziv).HasMaxLength(50).IsRequired();
            e.Property(r => r.Opis).HasMaxLength(200);
        });

        mb.Entity<WebClanstvo>(e =>
        {
            e.ToTable("tbl_web_clanstvo");
            e.HasKey(c => c.IdClanstva);
            e.Property(c => c.Napomena).HasMaxLength(500);
            e.HasOne(c => c.Licenca)
             .WithMany()
             .HasForeignKey(c => c.IdWebLicence);
            e.HasOne(c => c.Rola)
             .WithMany()
             .HasForeignKey(c => c.IdWebRole);
        });

    }
}
