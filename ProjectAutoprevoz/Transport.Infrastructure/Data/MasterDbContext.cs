using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;

namespace Transport.Infrastructure.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Licenca>     Licence      { get; set; }
    public DbSet<WebKorisnik> WebKorisnici { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Licenca>(e =>
        {
            e.ToTable("tbl_licence");
            e.HasKey(l => l.IdLicence);
            e.Property(l => l.Naziv).HasMaxLength(200);
            e.Property(l => l.ConnectionString).HasMaxLength(500);
        });

        mb.Entity<WebKorisnik>(e =>
        {
            e.ToTable("tbl_web_korisnici");
            e.HasKey(w => w.IdKorisnika);
            e.Property(w => w.Ime).HasMaxLength(100);
            e.Property(w => w.Email).HasMaxLength(200);
            e.Property(w => w.LozinkaHash).HasMaxLength(500);
            e.HasOne(w => w.Licenca)
             .WithMany()
             .HasForeignKey(w => w.IdLicence);
        });

    }
}
