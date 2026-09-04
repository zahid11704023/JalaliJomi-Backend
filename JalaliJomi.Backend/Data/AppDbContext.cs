using Microsoft.EntityFrameworkCore;
using JalaliJomi.Backend.Models;

namespace JalaliJomi.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<PropertyOwner> PropertyOwners { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Administrator> Administrators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Listing -> PropertyOwner (Owner)
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Owner)
                .WithMany(po => po.Listings)
                .HasForeignKey(l => l.PropertyOwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Listing -> Property (one-to-one)
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Property)
                .WithOne(p => p.Listing)
                .HasForeignKey<Listing>(l => l.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favourite -> RegisteredUser (User)
            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.RegisteredUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favourite -> Listing
            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.Listing)
                .WithMany(l => l.Favourites)
                .HasForeignKey(f => f.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ContactMessage: la PK si chiama MessageId, non segue la convenzione
            modelBuilder.Entity<ContactMessage>()
                .HasKey(cm => cm.MessageId);

            // ContactMessage -> RegisteredUser (Sender)
            modelBuilder.Entity<ContactMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactMessage -> Listing
            modelBuilder.Entity<ContactMessage>()
                .HasOne(cm => cm.Listing)
                .WithMany(l => l.ContactMessages)
                .HasForeignKey(cm => cm.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}