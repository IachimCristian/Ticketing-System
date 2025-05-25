using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services.Commands;

namespace TicketingSystem.Core.Services
{
    public class TicketPurchaseService : TicketViewService
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly PaymentStrategyFactory _paymentStrategyFactory;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly ISeatMapService _seatMapService;
        private readonly ICustomerNotificationService _customerNotificationService;

        public TicketPurchaseService(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            INotificationSubject notificationService,
            IPaymentService paymentService,
            IUserRepository<Customer> customerRepository,
            PaymentStrategyFactory paymentStrategyFactory,
            IRepository<Payment> paymentRepository,
            ISeatMapService seatMapService,
            ICustomerNotificationService customerNotificationService)
            : base(ticketRepository, eventRepository, notificationService)
        {
            _paymentService = paymentService;
            _customerRepository = customerRepository;
            _paymentStrategyFactory = paymentStrategyFactory;
            _paymentRepository = paymentRepository;
            _seatMapService = seatMapService;
            _customerNotificationService = customerNotificationService;
        }

        public async Task<Ticket> PurchaseTicketAsync(
            Guid eventId,
            Guid customerId,
            decimal price,
            int? seatRow,
            int? seatColumn,
            string paymentMethod)
        {
            // Get event and customer
            var @event = await _eventRepository.GetByIdAsync(eventId);
            if (@event == null)
            {
                throw new InvalidOperationException($"Event not found with ID: {eventId}");
            }

            if (!@event.IsActive)
            {
                throw new InvalidOperationException("Cannot purchase tickets for inactive events");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer not found with ID: {customerId}");
            }

            // Check if specific seat is requested and validate availability
            if (seatRow.HasValue && seatColumn.HasValue)
            {
                bool isSeatAvailable = await _seatMapService.IsSeatAvailableAsync(eventId, seatRow.Value, seatColumn.Value);
                if (!isSeatAvailable)
                {
                    throw new InvalidOperationException($"Seat {seatRow}-{seatColumn} is not available for this event");
                }
            }

            // Check general ticket availability
            if (!await _eventRepository.IsTicketAvailableAsync(eventId))
            {
                throw new InvalidOperationException("No tickets available for this event");
            }

            // Create ticket - only set EventId, not the Event navigation property to avoid tracking conflicts
            var ticket = new Ticket
            {
                EventId = eventId,
                CustomerId = customerId,
                Price = price,
                Status = "Available",
                SeatRow = seatRow,
                SeatColumn = seatColumn
            };

            // Get payment strategy
            var paymentStrategy = _paymentStrategyFactory.CreatePaymentStrategy(paymentMethod);

            // Create and execute purchase command
            var purchaseCommand = new PurchaseTicketCommand(
                ticket,
                customer,
                @event,
                paymentStrategy,
                _notificationService);

            await purchaseCommand.ExecuteAsync();

            // Save payment first
            if (ticket.Payment != null)
            {
                await _paymentRepository.AddAsync(ticket.Payment);
                await _paymentRepository.SaveChangesAsync();
            }

            // Save ticket
            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            // Send notifications using the new customer notification service
            // This will check preferences and send appropriate notifications
            await _customerNotificationService.SendTicketPurchaseNotificationAsync(customerId, ticket, @event);

            // Also send through the old observer pattern for email/SMS (for backward compatibility)
            await _notificationService.NotifyObserversAsync("TicketPurchased", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventTitle = @event.Title,
                EventDate = @event.StartDate,
                CustomerEmail = customer.Email,
                CustomerPhone = customer.Phone,
                CustomerId = customerId,
                Amount = price,
                PaymentMethod = paymentMethod
            });

            return ticket;
        }
    }
} 