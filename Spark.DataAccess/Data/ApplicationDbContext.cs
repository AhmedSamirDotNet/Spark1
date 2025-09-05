using Microsoft.EntityFrameworkCore;
using Spark.Models;

namespace Spark.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Billboard> Billboards { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client (1) <-> (many) Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Client)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Billboard (1) <-> (many) Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Billboard)
                .WithMany(bb => bb.Bookings)
                .HasForeignKey(b => b.BillboardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-one relationship between Billboard and Address
            // Use ONLY ONE of these configurations (choose based on your needs):

            // OPTION 1: If you want to KEEP the address when deleting billboard (Recommended based on your requirement)
            modelBuilder.Entity<Billboard>()
                .HasOne(b => b.Address)
                .WithOne(a => a.Billboard)
                .HasForeignKey<Billboard>(b => b.AddressId)
                .OnDelete(DeleteBehavior.Restrict); // Address won't be deleted when billboard is deleted

            modelBuilder.Entity<Billboard>()
                .HasIndex(b => b.AddressId)
                .IsUnique();

            // ContactUs (many) -> (1) Billboard (optional relationship)
            modelBuilder.Entity<ContactUs>()
                .HasOne(c => c.Billboard)
                .WithMany() // Billboard doesn't need navigation back to ContactUs
                .HasForeignKey(c => c.BillboardId)
                .OnDelete(DeleteBehavior.SetNull); // If billboard is deleted, set BillboardId to null
        }
    }
}