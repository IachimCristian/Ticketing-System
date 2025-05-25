using System;
using System.Collections.Generic;

namespace TicketingSystem.Core.Entities
{
    public class Customer : User
    {
        public string Phone { get; set; }
        public string Address { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        
        // Notification-related navigation properties
        public virtual ICollection<CustomerNotification> Notifications { get; set; }
        public virtual NotificationPreferences NotificationPreferences { get; set; }
        
        public Customer()
        {
            Tickets = new List<Ticket>();
            Notifications = new List<CustomerNotification>();
        }
    }
} 