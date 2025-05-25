using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we already have users (to avoid duplicate seeding)
            if (await context.Administrators.AnyAsync() || 
                await context.Organizers.AnyAsync() || 
                await context.Customers.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Create test users as specified in the report
            await SeedTestUsers(context);
            
            // No sample events - let the professor see an empty events list
        }

        private static async Task SeedTestUsers(AppDbContext context)
        {
            // Administrator - exactly as specified in the report
            var admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@ticksy.com",
                Password = "Admin123!", // Plain text for testing as specified
                Role = "SuperAdmin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Organizer - exactly as specified in the report
            var organizer = new Organizer
            {
                Id = Guid.NewGuid(),
                Username = "organizer",
                Email = "organizer@ticksy.com",
                Password = "Organizer123!", // Plain text for testing as specified
                OrganizationName = "Test Events Company",
                Description = "A test organization for event management",
                ContactPhone = "+1234567890",
                CreatedAt = DateTime.UtcNow
            };

            // Customer - exactly as specified in the report
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Username = "customer",
                Email = "customer@ticksy.com",
                Password = "Customer123!", // Plain text for testing as specified
                Phone = "+1987654321",
                Address = "123 Test Street, Test City, TC 12345",
                CreatedAt = DateTime.UtcNow
            };

            // Add users to context
            context.Administrators.Add(admin);
            context.Organizers.Add(organizer);
            context.Customers.Add(customer);

            await context.SaveChangesAsync();
        }
    }
} 