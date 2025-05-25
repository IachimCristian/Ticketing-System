using System;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; }

        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }

        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Available, Sold, Cancelled

        public int? SeatRow { get; set; }
        public int? SeatColumn { get; set; }

        public DateTime PurchaseDate { get; set; }
        public DateTime? CancelDate { get; set; }

        // Payment information
        public Guid? PaymentId { get; set; }
        public virtual Payment Payment { get; set; }

        // Refund information
        public Guid? RefundId { get; set; }
        
        [StringLength(20)]
        public string? RefundStatus { get; set; } // Approved, Denied, null for pending
        
        public DateTime? RefundProcessDate { get; set; }

        [StringLength(500)]
        public string? QRCode { get; set; }

        // Seat information
        public Guid? SeatId { get; set; }
        
        public Ticket()
        {
            Id = Guid.NewGuid();
            Status = "Available";
            PurchaseDate = DateTime.UtcNow;
            TicketNumber = GenerateTicketNumber();
        }
        
        private string GenerateTicketNumber()
        {
            // Format: TIX-YYMMDD-XXXX (16 chars)
            // YY = year, MM = month, DD = day, XXXX = random hex
            return $"TIX-{DateTime.UtcNow.ToString("yyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
} 