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
        public DbSet<CustomerNotification> CustomerNotifications { get; set; }
        public DbSet<NotificationPreferences> NotificationPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal properties
            modelBuilder.Entity<Event>()
                .Property(e => e.TicketPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");

            // Configure inheritance
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Organizer>().ToTable("Organizers");
            modelBuilder.Entity<Administrator>().ToTable("Administrators");

            // Configure Event entity
            modelBuilder.Entity<Event>()
                .Property(e => e.ImageUrl)
                .IsRequired(false);

            // Configure Ticket relationships
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Customer)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Payment)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PaymentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SeatMap relationships
            modelBuilder.Entity<SeatMap>()
                .HasMany(sm => sm.Sections)
                .WithOne()
                .HasForeignKey(s => s.SeatMapId);

            // Configure CustomerNotification relationships
            modelBuilder.Entity<CustomerNotification>()
                .HasOne(n => n.Customer)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerNotification>()
                .HasOne(n => n.Event)
                .WithMany()
                .HasForeignKey(n => n.EventId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CustomerNotification>()
                .HasOne(n => n.Ticket)
                .WithMany()
                .HasForeignKey(n => n.TicketId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure NotificationPreferences relationships
            modelBuilder.Entity<NotificationPreferences>()
                .HasOne(np => np.Customer)
                .WithOne(c => c.NotificationPreferences)
                .HasForeignKey<NotificationPreferences>(np => np.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure all decimal properties to use decimal(18,2)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
} 