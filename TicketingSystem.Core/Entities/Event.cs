using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TicketingSystem.Core.Entities
{
    public class Event
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        public decimal TicketPrice { get; set; }
<<<<<<< HEAD
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public Guid OrganizerId { get; set; }
        public virtual Organizer? Organizer { get; set; }
=======
        
        public int Capacity { get; set; }

        public Guid OrganizerId { get; set; }
        
        public bool IsActive { get; set; }

        public virtual Organizer Organizer { get; set; }
        
>>>>>>> a1f2cea (Fixed issues)
        public virtual ICollection<Ticket> Tickets { get; set; }

        public Event()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            Tickets = new List<Ticket>();
        }
    }
} 