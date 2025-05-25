using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class TicketCancellationService : TicketViewService
    {
        private readonly IPaymentService _paymentService;

        public TicketCancellationService(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            INotificationSubject notificationService,
            IPaymentService paymentService)
            : base(ticketRepository, eventRepository, notificationService)
        {
            _paymentService = paymentService;
        }

        public async Task<bool> CancelTicketAsync(Guid ticketId, Guid customerId)
        {
            // Get ticket without tracking to avoid conflicts
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.CustomerId != customerId)
            {
                throw new InvalidOperationException("Ticket not found or does not belong to the customer.");
            }

            if (ticket.Status != "Sold")
            {
                throw new InvalidOperationException("Only sold tickets can be cancelled.");
            }

            // Get event separately to avoid tracking issues
            var @event = ticket.Event ?? await _eventRepository.GetByIdAsync(ticket.EventId);
            if (@event == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            // Check if the event has already started
            if (@event.StartDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel tickets for events that have already started.");
            }

            // Process refund if payment exists
            if (ticket.PaymentId.HasValue)
            {
                await _paymentService.ProcessRefundAsync(ticket.PaymentId.Value, ticket.Price);
            }

            // Create a new ticket instance with only the fields we need to update
            var ticketToUpdate = new Ticket
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventId = ticket.EventId,
                CustomerId = ticket.CustomerId,
                Price = ticket.Price,
                Status = "Cancelled",
                CancelDate = DateTime.UtcNow,
                PurchaseDate = ticket.PurchaseDate,
                PaymentId = ticket.PaymentId,
                RefundId = ticket.RefundId,
                RefundStatus = ticket.RefundStatus,
                RefundProcessDate = ticket.RefundProcessDate,
                QRCode = ticket.QRCode,
                SeatRow = ticket.SeatRow,
                SeatColumn = ticket.SeatColumn,
                SeatId = ticket.SeatId
            };

            await _ticketRepository.UpdateAsync(ticketToUpdate);
            await _ticketRepository.SaveChangesAsync();

            // Notify about cancellation - use the original ticket data for notification
            await _notificationService.NotifyObserversAsync("TicketCancelled", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventTitle = @event.Title,
                CustomerEmail = ticket.Customer?.Email ?? "Unknown",
                Amount = ticket.Price
            });

            return true;
        }
    }
} 