using System;

namespace TicketingSystem.Core.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string QRCode { get; set; }
        public string Status { get; set; } // Available, Sold, Used, Cancelled
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public Guid? PaymentId { get; set; }
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
            return $"TIX-{DateTime.UtcNow.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
} 