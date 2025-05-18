using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Commands
{
    public class PurchaseTicketCommand : ICommand
    {
        private readonly Ticket _ticket;
        private readonly Customer _customer;
        private readonly Event _event;
        private readonly IPaymentStrategy _paymentStrategy;
        private readonly INotificationSubject _notificationService;
        private Payment _payment;
        
        public PurchaseTicketCommand(
            Ticket ticket, 
            Customer customer, 
            Event @event, 
            IPaymentStrategy paymentStrategy,
            INotificationSubject notificationService)
        {
            _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _event = @event ?? throw new ArgumentNullException(nameof(@event));
            _paymentStrategy = paymentStrategy ?? throw new ArgumentNullException(nameof(paymentStrategy));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }
        
        public async Task ExecuteAsync()
        {
            // Process payment
            _payment = await _paymentStrategy.ProcessPaymentAsync(_customer, _ticket.Price);
            
            // Update ticket status
            _ticket.Status = "Sold";
            _ticket.CustomerId = _customer.Id;
            _ticket.PaymentId = _payment.Id;
            
            // Generate QR code
            _ticket.QRCode = QRCodeGenerator.Instance.GenerateQRCode(
                _ticket.Id, 
                _event.Title, 
                _event.StartDate);
            
            // Notify observers
            await _notificationService.NotifyObserversAsync("TicketPurchased", new
            {
                TicketId = _ticket.Id,
                TicketNumber = _ticket.TicketNumber,
                EventTitle = _event.Title,
                EventDate = _event.StartDate,
                CustomerEmail = _customer.Email,
                CustomerPhone = _customer.Phone,
                Amount = _ticket.Price,
                PaymentMethod = _payment.PaymentMethod
            });
        }
        
        public async Task UndoAsync()
        {
            if (_payment != null)
            {
                // Refund payment
                await _paymentStrategy.RefundPaymentAsync(_payment);
                
                // Update ticket status
                _ticket.Status = "Cancelled";
                
                // Notify observers
                await _notificationService.NotifyObserversAsync("TicketCancelled", new
                {
                    TicketId = _ticket.Id,
                    TicketNumber = _ticket.TicketNumber,
                    EventTitle = _event.Title,
                    CustomerEmail = _customer.Email
                });
            }
        }
    }
} 