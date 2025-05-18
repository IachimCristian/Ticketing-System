using System;
using System.Collections.Generic;

namespace TicketingSystem.Core.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; } // Pending, Completed, Failed, Refunded
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        
        public Payment()
        {
            Id = Guid.NewGuid();
            PaymentDate = DateTime.UtcNow;
            Status = "Pending";
            Tickets = new List<Ticket>();
        }
    }
} 