using System;
using System.Collections.Generic;


namespace TicketingSystem.Core.Entities
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public decimal TicketPrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public Guid OrganizerId { get; set; }
        public virtual Organizer? Organizer { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

        public Event()
        {
            Id = Guid.NewGuid();
            Tickets = new List<Ticket>();
            IsActive = true;
        }
    }
} 