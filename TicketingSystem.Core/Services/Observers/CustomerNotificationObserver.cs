using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Observers
{
    public class CustomerNotificationObserver : INotificationObserver
    {
        private readonly ICustomerNotificationService _customerNotificationService;
        private readonly ILogger<CustomerNotificationObserver> _logger;

        public CustomerNotificationObserver(
            ICustomerNotificationService customerNotificationService,
            ILogger<CustomerNotificationObserver> logger)
        {
            _customerNotificationService = customerNotificationService;
            _logger = logger;
        }

        public async Task UpdateAsync(string eventType, object data)
        {
            try
            {
                switch (eventType)
                {
                    case "TicketPurchased":
                        await HandleTicketPurchasedAsync(data);
                        break;
                    case "EventUpdated":
                        await HandleEventUpdatedAsync(data);
                        break;
                    case "EventCancelled":
                        await HandleEventCancelledAsync(data);
                        break;
                    case "TicketCancelled":
                        await HandleTicketCancelledAsync(data);
                        break;
                    case "RefundProcessed":
                        await HandleRefundProcessedAsync(data);
                        break;
                    default:
                        _logger.LogDebug("Unhandled event type for customer notifications: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification event {EventType}", eventType);
            }
        }

        private async Task HandleTicketPurchasedAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                // Extract data from the notification
                Guid customerId = ticketData.CustomerId;
                Guid ticketId = ticketData.TicketId;
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                DateTime eventDate = ticketData.EventDate;
                decimal amount = ticketData.Amount;

                // Create in-app notification
                await _customerNotificationService.CreateNotificationAsync(
                    customerId,
                    "Ticket Purchase Confirmed",
                    $"Your ticket for '{eventTitle}' has been successfully purchased. Ticket number: {ticketNumber}. Amount: ${amount:F2}",
                    "TicketPurchase",
                    "High",
                    null, // EventId would need to be passed in data
                    ticketId,
                    $"/Tickets/Confirmation/{ticketId}",
                    "View Ticket"
                );

                _logger.LogInformation("Created ticket purchase notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ticket purchased notification");
            }
        }

        private async Task HandleEventUpdatedAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                Guid customerId = eventData.CustomerId;
                Guid eventId = eventData.EventId;
                string eventTitle = eventData.EventTitle;
                string updateMessage = eventData.UpdateMessage;

                await _customerNotificationService.CreateNotificationAsync(
                    customerId,
                    $"Event Update: {eventTitle}",
                    updateMessage,
                    "EventUpdate",
                    "Medium",
                    eventId,
                    null,
                    $"/Events/Details/{eventId}",
                    "View Event"
                );

                _logger.LogInformation("Created event update notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event updated notification");
            }
        }

        private async Task HandleEventCancelledAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                Guid customerId = eventData.CustomerId;
                Guid eventId = eventData.EventId;
                string eventTitle = eventData.EventTitle;
                DateTime eventDate = eventData.EventDate;

                await _customerNotificationService.CreateNotificationAsync(
                    customerId,
                    $"Event Cancelled: {eventTitle}",
                    $"Unfortunately, the event '{eventTitle}' scheduled for {eventDate:MMMM d, yyyy} has been cancelled. You will receive a full refund for your tickets.",
                    "EventCancellation",
                    "Urgent",
                    eventId,
                    null,
                    $"/Events/Details/{eventId}",
                    "View Details"
                );

                _logger.LogInformation("Created event cancellation notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event cancelled notification");
            }
        }

        private async Task HandleTicketCancelledAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                Guid customerId = ticketData.CustomerId ?? Guid.Empty;
                if (customerId == Guid.Empty) return;

                Guid ticketId = ticketData.TicketId;
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                decimal amount = ticketData.Amount;

                await _customerNotificationService.CreateNotificationAsync(
                    customerId,
                    "Ticket Cancelled",
                    $"Your ticket {ticketNumber} for '{eventTitle}' has been cancelled. Refund of ${amount:F2} is being processed.",
                    "TicketCancellation",
                    "High",
                    null,
                    ticketId,
                    $"/Tickets/Confirmation/{ticketId}",
                    "View Details"
                );

                _logger.LogInformation("Created ticket cancellation notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ticket cancelled notification");
            }
        }

        private async Task HandleRefundProcessedAsync(object data)
        {
            try
            {
                dynamic refundData = data;
                
                Guid customerId = refundData.CustomerId;
                Guid ticketId = refundData.TicketId;
                string ticketNumber = refundData.TicketNumber;
                decimal refundAmount = refundData.RefundAmount;

                await _customerNotificationService.CreateNotificationAsync(
                    customerId,
                    "Refund Processed",
                    $"Your refund of ${refundAmount:F2} for ticket {ticketNumber} has been processed and will appear in your account within 3-5 business days.",
                    "Refund",
                    "High",
                    null,
                    ticketId,
                    $"/Tickets/Confirmation/{ticketId}",
                    "View Details"
                );

                _logger.LogInformation("Created refund processed notification for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling refund processed notification");
            }
        }
    }
} 