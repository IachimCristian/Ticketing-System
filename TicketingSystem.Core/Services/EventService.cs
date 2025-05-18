using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services.Commands;

namespace TicketingSystem.Core.Services
{
    public class EventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly INotificationSubject _notificationService;
        
        public EventService(
            IEventRepository eventRepository,
            ITicketRepository ticketRepository,
            INotificationSubject notificationService)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }
        
        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _eventRepository.GetUpcomingEventsAsync();
        }
        
        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id);
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
            {
                return false;
            }
            
            var command = new CancelEventCommand(@event, _notificationService);
            await command.ExecuteAsync();
            
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
            return await _ticketRepository.GetTicketsByEventAsync(eventId);
        }
    }
} 