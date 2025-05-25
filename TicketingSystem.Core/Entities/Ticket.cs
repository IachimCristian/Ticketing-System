using System;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TicketNumber { get; set; }

        public Guid EventId { get; set; }

        public Guid CustomerId { get; set; }

        public DateTime PurchaseDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public decimal Price { get; set; }

        [StringLength(500)]
        public string QRCode { get; set; }

        // Payment information
        public Guid PaymentId { get; set; }
        
        // Refund information
        public Guid? RefundId { get; set; }
        
        // Seat information
        public Guid? SeatId { get; set; }
        public int? SeatRow { get; set; }
        public int? SeatColumn { get; set; }
        
        // Navigation properties
        public virtual Event Event { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Payment Payment { get; set; }

        public Ticket()
        {
            Id = Guid.NewGuid();
            PurchaseDate = DateTime.UtcNow;
            TicketNumber = GenerateTicketNumber();
            Status = "Available";
        }
        
        private string GenerateTicketNumber()
        {
            // Format: TIX-YYMMDD-XXXX (16 chars)
            // YY = year, MM = month, DD = day, XXXX = random hex
            return $"TIX-{DateTime.UtcNow.ToString("yyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
} 