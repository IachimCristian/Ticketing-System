using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<Event> GetEventByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
        Task<Event> CreateEventAsync(Event @event);
        Task<bool> UpdateEventAsync(Event @event);
        Task<bool> CancelEventAsync(Guid eventId);
        Task<bool> IsTicketAvailableAsync(Guid eventId);
        Task<IEnumerable<Ticket>> GetEventTicketsAsync(Guid eventId);
    }
} 