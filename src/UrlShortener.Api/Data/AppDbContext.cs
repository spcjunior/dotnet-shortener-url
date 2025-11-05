using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Url> Urls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Url>(entity =>
        {
            entity.ToTable("urls");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ShortCode)
                .HasColumnName("short_code")
                .HasMaxLength(10)
                .IsRequired();

            entity.HasIndex(e => e.ShortCode)
                .IsUnique();

            entity.Property(e => e.OriginalUrl)
                .HasColumnName("original_url")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");
        });

        // Configurar sequence para iniciar em 916132832
        modelBuilder.HasSequence<long>("url_id_seq")
            .StartsAt(916132832)
            .IncrementsBy(1);

        modelBuilder.Entity<Url>()
            .Property(e => e.Id)
            .HasDefaultValueSql("nextval('url_id_seq')");
    }
}
