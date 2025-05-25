using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class TicketCancellationService : TicketViewService
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserRepository<Customer> _customerRepository;

        public TicketCancellationService(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            INotificationSubject notificationService,
            IPaymentService paymentService,
            IUserRepository<Customer> customerRepository)
            : base(ticketRepository, eventRepository, notificationService)
        {
            _paymentService = paymentService;
            _customerRepository = customerRepository;
        }

        public async Task<bool> CancelTicketAsync(Guid ticketId, Guid customerId)
        {
            // First, get ticket details using base service functionality
            var ticket = await GetTicketDetailsAsync(ticketId, customerId);

            if (ticket.Status != "Sold")
            {
                throw new InvalidOperationException("Only sold tickets can be cancelled");
            }

            // Event date check - can't cancel too close to event
            if (ticket.Event?.StartDate.AddDays(-1) <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel tickets within 24 hours of the event");
            }

            // Load the customer if not loaded
            if (ticket.Customer == null)
            {
                ticket.Customer = await _customerRepository.GetByIdAsync(ticket.CustomerId);
            }

            // Process refund if applicable
            if (ticket.Payment != null && ticket.Event.StartDate.AddDays(-2) > DateTime.UtcNow)
            {
                var refundId = await _paymentService.ProcessRefundAsync(ticket.PaymentId, ticket.Price);
                ticket.RefundId = refundId;
            }

            // Update ticket status
            ticket.Status = "Cancelled";
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            // Notify about cancellation
            await _notificationService.NotifyObserversAsync("TicketCancelled", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventTitle = ticket.Event?.Title ?? "Unknown Event",
                CustomerEmail = ticket.Customer?.Email ?? "Unknown Email",
                RefundIssued = ticket.RefundId.HasValue
            });

            return true;
        }
    }
} 