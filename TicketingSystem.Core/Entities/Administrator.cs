using System;

namespace TicketingSystem.Core.Entities
{
    public class Administrator : User
    {
        public string Role { get; set; }
        public bool IsActive { get; set; }
        
        public Administrator()
        {
            IsActive = true;
        }
    }
} 