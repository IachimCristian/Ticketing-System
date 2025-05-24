using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services.Commands;

namespace TicketingSystem.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly INotificationSubject _notificationService;
        
        public EventService(
            IEventRepository eventRepository,
            INotificationSubject notificationService,
            ITicketRepository ticketRepository = null)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _ticketRepository = ticketRepository;
        }
        
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllAsync();
        }
        
        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }
        
        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            try
            {
                return await _eventRepository.GetUpcomingEventsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException("Error retrieving upcoming events", ex);
            }
        }
        
        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            return await _eventRepository.GetEventsByOrganizerAsync(organizerId);
        }
        
        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            return await _eventRepository.SearchEventsAsync(searchTerm);
        }
        
        public async Task<Event> CreateEventAsync(Event @event)
        {
            await _eventRepository.AddAsync(@event);
            await _eventRepository.SaveChangesAsync();
            
            await _notificationService.NotifyObserversAsync("EventCreated", new
            {
                EventId = @event.Id,
                EventTitle = @event.Title,
                EventDate = @event.StartDate,
                OrganizerId = @event.OrganizerId
            });
            
            return @event;
        }
        
        public async Task<bool> UpdateEventAsync(Event @event)
        {
            await _eventRepository.UpdateAsync(@event);
            await _eventRepository.SaveChangesAsync();
            
            await _notificationService.NotifyObserversAsync("EventUpdated", new
            {
                EventId = @event.Id,
                EventTitle = @event.Title,
                EventDate = @event.StartDate,
                OrganizerId = @event.OrganizerId
            });
            
            return true;
        }
        
        public async Task<bool> CancelEventAsync(Guid eventId)
        {
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
                return false;

            @event.IsActive = false;
            await _eventRepository.UpdateAsync(@event);
            await _eventRepository.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> IsTicketAvailableAsync(Guid eventId)
        {
            return await _eventRepository.IsTicketAvailableAsync(eventId);
        }
        
        public async Task<IEnumerable<Ticket>> GetEventTicketsAsync(Guid eventId)
        {
            if (_ticketRepository == null)
            {
                throw new InvalidOperationException("Ticket repository is not available");
            }
            
            return await _ticketRepository.GetTicketsByEventAsync(eventId);
        }
    }
} 