using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _dbSet
                .Where(e => e.StartDate > DateTime.UtcNow && e.IsActive)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            return await _dbSet
                .Where(e => e.OrganizerId == organizerId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetUpcomingEventsAsync();

            return await _dbSet
                .Where(e => e.IsActive && 
                           (e.Title.Contains(searchTerm) || 
                            e.Description.Contains(searchTerm) || 
                            e.Location.Contains(searchTerm)))
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<bool> IsTicketAvailableAsync(Guid eventId)
        {
            var @event = await _dbSet
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null || !@event.IsActive)
                return false;

            int soldTickets = @event.Tickets.Count(t => t.Status == "Sold" || t.Status == "Reserved");
            return soldTickets < @event.Capacity;
        }
    }
} 