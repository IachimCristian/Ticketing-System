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
            var events = await _dbSet
                .Where(e => e.StartDate > DateTime.UtcNow && e.IsActive)
                .Include(e => e.Tickets.Where(t => t.Status != "Cancelled"))
                    .ThenInclude(t => t.Customer)
                .OrderBy(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            return events;
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            var events = await _dbSet
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.Tickets.Where(t => t.Status != "Cancelled"))
                    .ThenInclude(t => t.Customer)
                .OrderByDescending(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            return events;
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetUpcomingEventsAsync();

            var events = await _dbSet
                .Where(e => e.IsActive && 
                           (e.Title.Contains(searchTerm) || 
                            e.Description.Contains(searchTerm) || 
                            e.Location.Contains(searchTerm)))
                .Include(e => e.Tickets.Where(t => t.Status != "Cancelled"))
                    .ThenInclude(t => t.Customer)
                .OrderBy(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            return events;
        }

        public async Task<bool> IsTicketAvailableAsync(Guid eventId)
        {
            var ticketCount = await _context.Tickets
                .CountAsync(t => t.EventId == eventId && t.Status != "Cancelled");

            var @event = await _dbSet
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null || !@event.IsActive)
                return false;

            return (@event.Capacity - ticketCount) > 0;
        }
    }
} 