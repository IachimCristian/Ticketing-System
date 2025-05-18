using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Commands
{
    public class CancelEventCommand : ICommand
    {
        private readonly Event _event;
        private readonly INotificationSubject _notificationService;
        private readonly bool _previousState;
        
        public CancelEventCommand(Event @event, INotificationSubject notificationService)
        {
            _event = @event ?? throw new ArgumentNullException(nameof(@event));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _previousState = @event.IsActive;
        }
        
        public async Task ExecuteAsync()
        {
            // Mark event as inactive
            _event.IsActive = false;
            
            // Notify observers
            await _notificationService.NotifyObserversAsync("EventCancelled", new
            {
                EventId = _event.Id,
                EventTitle = _event.Title,
                EventDate = _event.StartDate,
                OrganizerId = _event.OrganizerId
            });
        }
        
        public async Task UndoAsync()
        {
            // Restore previous state
            _event.IsActive = _previousState;
            
            if (_previousState)
            {
                // Notify observers if the event is being reactivated
                await _notificationService.NotifyObserversAsync("EventReactivated", new
                {
                    EventId = _event.Id,
                    EventTitle = _event.Title,
                    EventDate = _event.StartDate,
                    OrganizerId = _event.OrganizerId
                });
            }
        }
    }
} 