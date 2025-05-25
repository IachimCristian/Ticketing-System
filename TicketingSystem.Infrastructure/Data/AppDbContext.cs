using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SeatMap> SeatMaps { get; set; }
        public DbSet<SeatSection> SeatSections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Organizer>().ToTable("Organizers");
            modelBuilder.Entity<Administrator>().ToTable("Administrators");
            
            // Configure Event entity
            modelBuilder.Entity<Event>()
                .Property(e => e.ImageUrl)
                .IsRequired(false); // Make ImageUrl nullable
            
            // Configure SeatMap and SeatSection
            modelBuilder.Entity<SeatMap>().ToTable("SeatMaps");
            modelBuilder.Entity<SeatSection>().ToTable("SeatSections");
            
            // Configure relationship between SeatMap and SeatSection
            modelBuilder.Entity<SeatSection>()
                .HasOne<SeatMap>()
                .WithMany(sm => sm.Sections)
                .HasForeignKey("SeatMapId");
        }
    }
} 