using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Models;

namespace VetClinicMvc.Data;
public class VetDbContext(DbContextOptions<VetDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Pet> Pets => Set<Pet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "dbo");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Role).HasColumnName("role").HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSUTCDATETIME()");
        });
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("pets", "dbo");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OwnerId).HasColumnName("owner_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Species).HasColumnName("species").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Breed).HasColumnName("breed").HasMaxLength(100);
            entity.Property(x => x.BirthDate).HasColumnName("birth_date").HasColumnType("date");
            entity.Property(x => x.Sex).HasColumnName("sex").HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
