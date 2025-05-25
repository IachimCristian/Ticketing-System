using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketingSystem.Infrastructure.Data.Migrations
{
    public partial class AddRefundColumnsToTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Add new columns for refund functionality
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundId')
                BEGIN
                    ALTER TABLE dbo.Tickets DROP COLUMN RefundId;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundStatus')
                BEGIN
                    ALTER TABLE dbo.Tickets DROP COLUMN RefundStatus;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'RefundProcessDate')
                BEGIN
                    ALTER TABLE dbo.Tickets DROP COLUMN RefundProcessDate;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tickets') AND name = 'CancelDate')
                BEGIN
                    ALTER TABLE dbo.Tickets DROP COLUMN CancelDate;
                END
            ");
        }
    }
} 