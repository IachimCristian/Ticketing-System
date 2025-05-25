using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Core.Services.Observers
{
    public class SMSNotificationObserver : INotificationObserver
    {
        private readonly ILogger<SMSNotificationObserver> _logger;

        public SMSNotificationObserver(ILogger<SMSNotificationObserver> logger)
        {
            _logger = logger;
        }

        public async Task UpdateAsync(string eventType, object data)
        {
            try
            {
                switch (eventType)
                {
                    case "TicketPurchased":
                        await SendTicketPurchaseConfirmationAsync(data);
                        break;
                    case "EventCancelled":
                        await SendEventCancellationNotificationAsync(data);
                        break;
                    case "EventReminder":
                        await SendEventReminderAsync(data);
                        break;
                    case "EventUpdated":
                        await SendEventUpdateNotificationAsync(data);
                        break;
                    case "RefundProcessed":
                        await SendRefundNotificationAsync(data);
                        break;
                    case "TicketCancelled":
                        await SendTicketCancellationNotificationAsync(data);
                        break;
                    default:
                        _logger.LogDebug("Unsupported event type for SMS notification: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification for event type: {EventType}", eventType);
            }
        }
        
        private async Task SendTicketPurchaseConfirmationAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                // Extract data
                string customerPhone = ticketData.CustomerPhone ?? "N/A";
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                DateTime eventDate = ticketData.EventDate;
                decimal amount = ticketData.Amount;

                // In a real application, this would send an actual SMS using a service like Twilio, AWS SNS, etc.
                var smsContent = $"Ticksy: Your ticket for '{eventTitle}' is confirmed! Ticket: {ticketNumber}, Date: {eventDate:MMM d, yyyy}, Amount: ${amount:F2}. Keep this confirmation safe!";

                _logger.LogInformation("SMS SENT to {Phone}: Ticket purchase confirmation for {EventTitle}", customerPhone, eventTitle);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                // Simulate SMS sending delay
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket purchase confirmation SMS");
            }
        }
        
        private async Task SendEventCancellationNotificationAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                string customerPhone = eventData.CustomerPhone ?? "N/A";
                string eventTitle = eventData.EventTitle;
                DateTime eventDate = eventData.EventDate;

                var smsContent = $"Ticksy: URGENT - Event '{eventTitle}' scheduled for {eventDate:MMM d, yyyy} has been CANCELLED. Full refund will be processed within 3-5 days. Sorry for the inconvenience.";

                _logger.LogInformation("SMS SENT to {Phone}: Event cancellation notice for {EventTitle}", customerPhone, eventTitle);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event cancellation SMS");
            }
        }
        
        private async Task SendEventReminderAsync(object data)
        {
            try
            {
                dynamic reminderData = data;
                
                string customerPhone = reminderData.CustomerPhone ?? "N/A";
                string eventTitle = reminderData.EventTitle;
                DateTime eventDate = reminderData.EventDate;
                string ticketNumber = reminderData.TicketNumber;

                var hoursUntil = (eventDate - DateTime.UtcNow).TotalHours;
                var timeText = hoursUntil > 24 ? $"{Math.Round(hoursUntil / 24)} days" : $"{Math.Round(hoursUntil)} hours";

                var smsContent = $"Ticksy: Reminder! '{eventTitle}' starts in {timeText}. Ticket: {ticketNumber}. Don't forget to attend!";

                _logger.LogInformation("SMS SENT to {Phone}: Event reminder for {EventTitle}", customerPhone, eventTitle);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event reminder SMS");
            }
        }

        private async Task SendEventUpdateNotificationAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                string customerPhone = eventData.CustomerPhone ?? "N/A";
                string eventTitle = eventData.EventTitle;
                string updateMessage = eventData.UpdateMessage;

                var smsContent = $"Ticksy: Update for '{eventTitle}': {updateMessage}. Check your account for full details.";

                _logger.LogInformation("SMS SENT to {Phone}: Event update for {EventTitle}", customerPhone, eventTitle);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event update SMS");
            }
        }

        private async Task SendRefundNotificationAsync(object data)
        {
            try
            {
                dynamic refundData = data;
                
                string customerPhone = refundData.CustomerPhone ?? "N/A";
                string ticketNumber = refundData.TicketNumber;
                decimal refundAmount = refundData.RefundAmount;

                var smsContent = $"Ticksy: Refund of ${refundAmount:F2} for ticket {ticketNumber} has been processed. Funds will appear in 3-5 business days.";

                _logger.LogInformation("SMS SENT to {Phone}: Refund notification for ticket {TicketNumber}", customerPhone, ticketNumber);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification SMS");
            }
        }

        private async Task SendTicketCancellationNotificationAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                string customerPhone = ticketData.CustomerPhone ?? "N/A";
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                decimal amount = ticketData.Amount;

                var smsContent = $"Ticksy: Ticket {ticketNumber} for '{eventTitle}' has been cancelled. Refund of ${amount:F2} will be processed within 3-5 days.";

                _logger.LogInformation("SMS SENT to {Phone}: Ticket cancellation confirmation for {TicketNumber}", customerPhone, ticketNumber);
                _logger.LogInformation("SMS Content: {Content}", smsContent);
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket cancellation SMS");
            }
        }
    }
} 