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

        public override async Task<Event> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbSet
                    .Include(e => e.Tickets)
                    .Include(e => e.Organizer)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            try
            {
                var events = await _dbSet
                    .Include(e => e.Tickets)
                    .Where(e => e.StartDate > DateTime.Now && e.IsActive)
                    .OrderBy(e => e.StartDate)
                    .AsNoTracking()
                    .ToListAsync();

                if (events == null || !events.Any())
                {
                    events = await _dbSet
                        .Include(e => e.Tickets)
                        .Where(e => e.IsActive)
                        .OrderBy(e => e.StartDate)
                        .AsNoTracking()
                        .ToListAsync();
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
                .Include(e => e.Tickets)
                .Where(e => e.OrganizerId == organizerId)
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
                .Include(e => e.Tickets)
                .Where(e => e.IsActive && 
                           (e.Title.Contains(searchTerm) || 
                            e.Description.Contains(searchTerm) || 
                            e.Location.Contains(searchTerm)))
                .OrderBy(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
                
            return events;
        }

        public async Task<bool> IsTicketAvailableAsync(Guid eventId)
        {
            var ticketCount = await _context.Tickets
                .CountAsync(t => t.EventId == eventId && t.Status == "Sold");

            var @event = await _dbSet
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null || !@event.IsActive)
                return false;

            return (@event.Capacity - ticketCount) > 0;
        }
    }
} 