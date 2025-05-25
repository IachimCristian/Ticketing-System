using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TicketingSystem.Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Add refund columns if they don't exist
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'CancelDate')
                BEGIN
                    ALTER TABLE dbo.Tickets ADD CancelDate datetime2 NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundProcessDate')
                BEGIN
                    ALTER TABLE dbo.Tickets ADD RefundProcessDate datetime2 NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundStatus')
                BEGIN
                    ALTER TABLE dbo.Tickets ADD RefundStatus nvarchar(20) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundId')
                BEGIN
                    ALTER TABLE dbo.Tickets ADD RefundId uniqueidentifier NULL;
                END
            ");
        }
    }
} 