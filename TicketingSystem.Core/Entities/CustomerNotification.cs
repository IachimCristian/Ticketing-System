using System;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities
{
    public class CustomerNotification
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // TicketPurchase, EventUpdate, EventCancellation, EventReminder, Refund, etc.

        [StringLength(50)]
        public string Priority { get; set; } // Low, Medium, High, Urgent

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // Optional: Link to related entities
        public Guid? EventId { get; set; }
        public virtual Event Event { get; set; }

        public Guid? TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }

        // Notification delivery preferences
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public DateTime? SmsSentAt { get; set; }

        [StringLength(500)]
        public string ActionUrl { get; set; } // URL for "View Ticket", "View Event", etc.

        [StringLength(100)]
        public string ActionText { get; set; } // "View Ticket", "Download PDF", etc.

        public CustomerNotification()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
            Priority = "Medium";
        }
    }
} 