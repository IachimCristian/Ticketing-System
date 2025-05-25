using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class TicketViewService
    {
        protected readonly ITicketRepository _ticketRepository;
        protected readonly IEventRepository _eventRepository;
        protected readonly INotificationSubject _notificationService;

        public TicketViewService(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            INotificationSubject notificationService)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _notificationService = notificationService;
        }

        public virtual async Task<Ticket> GetTicketDetailsAsync(Guid ticketId, Guid customerId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            
            if (ticket == null || ticket.CustomerId != customerId)
            {
                throw new InvalidOperationException("Ticket not found or unauthorized access");
            }

            // Ensure related entities are loaded
            if (ticket.Event == null)
            {
                ticket.Event = await _eventRepository.GetByIdAsync(ticket.EventId);
            }

            // Notify about ticket view
            await _notificationService.NotifyObserversAsync("TicketViewed", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventTitle = ticket.Event?.Title ?? "Unknown Event",
                ViewedAt = DateTime.UtcNow
            });

            return ticket;
        }
    }
} 