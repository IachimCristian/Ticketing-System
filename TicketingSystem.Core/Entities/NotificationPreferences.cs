using System;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities
{
    public class NotificationPreferences
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // Email notification preferences
        public bool EmailTicketPurchase { get; set; } = true;
        public bool EmailEventUpdates { get; set; } = true;
        public bool EmailEventCancellations { get; set; } = true;
        public bool EmailEventReminders { get; set; } = true;
        public bool EmailRefundUpdates { get; set; } = true;
        public bool EmailPromotions { get; set; } = false;

        // SMS notification preferences
        public bool SmsTicketPurchase { get; set; } = false;
        public bool SmsEventUpdates { get; set; } = false;
        public bool SmsEventCancellations { get; set; } = true;
        public bool SmsEventReminders { get; set; } = false;
        public bool SmsRefundUpdates { get; set; } = false;

        // In-app notification preferences
        public bool InAppTicketPurchase { get; set; } = true;
        public bool InAppEventUpdates { get; set; } = true;
        public bool InAppEventCancellations { get; set; } = true;
        public bool InAppEventReminders { get; set; } = true;
        public bool InAppRefundUpdates { get; set; } = true;
        public bool InAppPromotions { get; set; } = true;

        // Reminder timing preferences (hours before event)
        public int EventReminderHours { get; set; } = 24;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public NotificationPreferences()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 