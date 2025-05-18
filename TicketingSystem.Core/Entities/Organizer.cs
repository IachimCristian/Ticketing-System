using System;
using System.Collections.Generic;

namespace TicketingSystem.Core.Entities
{
    public class Organizer : User
    {
        public string OrganizationName { get; set; }
        public string Description { get; set; }
        public string ContactPhone { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        
        public Organizer()
        {
            Events = new List<Event>();
        }
    }
} 