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
            try
            {
                var events = await _dbSet
                    .Where(e => e.StartDate > DateTime.Now && e.IsActive)
                    .OrderBy(e => e.StartDate)
                    .AsNoTracking()
                    .ToListAsync();

                if (events == null || !events.Any())
                {
                    events = await _dbSet
                        .Where(e => e.IsActive)
                        .OrderBy(e => e.StartDate)
                        .AsNoTracking()
                        .ToListAsync();
                }

                foreach (var evt in events)
                {
                    var ticketCount = await _context.Set<Ticket>()
                        .CountAsync(t => t.EventId == evt.Id && t.Status != "Cancelled");
                    
                    evt.Tickets = new List<Ticket>();
                }
                    
                return events;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUpcomingEventsAsync: {ex.Message}");
                return new List<Event>();
            }
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            var events = await _dbSet
                .Where(e => e.OrganizerId == organizerId)
                .OrderByDescending(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            foreach (var evt in events)
            {
                var ticketCount = await _context.Set<Ticket>()
                    .CountAsync(t => t.EventId == evt.Id && t.Status != "Cancelled");
                    
                evt.Tickets = new List<Ticket>();
            }
                
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
                .OrderBy(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            foreach (var evt in events)
            {
                var ticketCount = await _context.Set<Ticket>()
                    .CountAsync(t => t.EventId == evt.Id && t.Status != "Cancelled");
                    
                evt.Tickets = new List<Ticket>();
            }
                
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