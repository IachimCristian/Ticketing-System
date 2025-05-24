using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        
        public DateTime TransactionDate { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        
        // For refunds, this points to the original payment
        public Guid? RelatedPaymentId { get; set; }
        
        // Navigation property
        public virtual Customer Customer { get; set; }
        
        public virtual ICollection<Ticket> Tickets { get; set; }
        
        public Payment()
        {
            Id = Guid.NewGuid();
            TransactionDate = DateTime.UtcNow;
            Status = "Pending";
            Tickets = new List<Ticket>();
        }
    }
} 