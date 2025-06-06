using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services.Commands;

namespace TicketingSystem.Core.Services
{
    public class TicketPurchaseFacade : ITicketPurchaseFacade
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly PaymentStrategyFactory _paymentStrategyFactory;
        private readonly INotificationSubject _notificationService;
        
        public TicketPurchaseFacade(
            IEventRepository eventRepository,
            ITicketRepository ticketRepository,
            IUserRepository<Customer> customerRepository,
            IRepository<Payment> paymentRepository,
            PaymentStrategyFactory paymentStrategyFactory,
            INotificationSubject notificationService)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _paymentStrategyFactory = paymentStrategyFactory ?? throw new ArgumentNullException(nameof(paymentStrategyFactory));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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
                throw new ArgumentException($"Event not found with ID: {eventId}");
            }
            
            if (!@event.IsActive)
            {
                throw new InvalidOperationException("Cannot purchase tickets for inactive events");
            }
            
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException($"Customer not found with ID: {customerId}");
            }
            
            // Check ticket availability
            if (!await _eventRepository.IsTicketAvailableAsync(eventId))
            {
                throw new InvalidOperationException("No tickets available for this event");
            }
            
            // Create ticket
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
            
            return ticket;
        }
        
        public async Task<bool> CancelTicketAsync(Guid ticketId, Guid customerId)
        {
            // Get ticket with related entities
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.CustomerId != customerId)
            {
                return false;
            }
            
            if (ticket.Status != "Sold")
            {
                return false;
            }
            
            // Load the event if not loaded
            if (ticket.Event == null)
            {
                ticket.Event = await _eventRepository.GetByIdAsync(ticket.EventId);
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
            
            ticket.Status = "Cancelled";
            
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();
            
            await _notificationService.NotifyObserversAsync("TicketCancelled", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                EventTitle = ticket.Event?.Title ?? "Unknown Event",
                CustomerEmail = ticket.Customer?.Email ?? "Unknown Email"
            });
            
            return true;
        }
    }
} 