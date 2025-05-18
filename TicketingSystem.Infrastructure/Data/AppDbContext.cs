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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Organizer>().ToTable("Organizers");
            modelBuilder.Entity<Administrator>().ToTable("Administrators");
        }
    }
} 