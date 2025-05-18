using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
        Task<bool> IsTicketAvailableAsync(Guid eventId);
    }
} 