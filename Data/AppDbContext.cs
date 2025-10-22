using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SparePartsWeb.Models;

namespace SparePartsWeb.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ✅ Add all your app entities here
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for Identity tables

            // SparePart mapping
            modelBuilder.Entity<SparePart>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Brand)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Price)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Quantity)
                      .HasDefaultValue(0);
            });

            // Optional: Configure others (if you want to be explicit)
            modelBuilder.Entity<Equipment>().HasKey(e => e.Id);
            modelBuilder.Entity<Maintenance>().HasKey(m => m.Id);
            modelBuilder.Entity<Vendor>().HasKey(v => v.Id);
        }
    }
}
