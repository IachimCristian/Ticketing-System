using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Core.Services.Observers
{
    public class EmailNotificationObserver : INotificationObserver
    {
        private readonly ILogger<EmailNotificationObserver> _logger;

        public EmailNotificationObserver(ILogger<EmailNotificationObserver> logger)
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
                    case "EventUpdated":
                        await SendEventUpdateNotificationAsync(data);
                        break;
                    case "EventReminder":
                        await SendEventReminderAsync(data);
                        break;
                    case "RefundProcessed":
                        await SendRefundNotificationAsync(data);
                        break;
                    case "TicketCancelled":
                        await SendTicketCancellationNotificationAsync(data);
                        break;
                    default:
                        _logger.LogDebug("Unsupported event type for email notification: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification for event type: {EventType}", eventType);
            }
        }
        
        private async Task SendTicketPurchaseConfirmationAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                // Extract data
                string customerEmail = ticketData.CustomerEmail;
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                DateTime eventDate = ticketData.EventDate;
                decimal amount = ticketData.Amount;
                string paymentMethod = ticketData.PaymentMethod;

                // In a real application, this would send an actual email using a service like SendGrid, SMTP, etc.
                var emailContent = $@"
                    Subject: Ticket Purchase Confirmation - {eventTitle}
                    
                    Dear Customer,
                    
                    Thank you for your purchase! Your ticket has been confirmed.
                    
                    Event: {eventTitle}
                    Date: {eventDate:MMMM d, yyyy 'at' h:mm tt}
                    Ticket Number: {ticketNumber}
                    Amount Paid: ${amount:F2}
                    Payment Method: {paymentMethod}
                    
                    Please keep this confirmation for your records.
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Ticket purchase confirmation for {EventTitle}", customerEmail, eventTitle);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                // Simulate email sending delay
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket purchase confirmation email");
            }
        }
        
        private async Task SendEventCancellationNotificationAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                string customerEmail = eventData.CustomerEmail;
                string eventTitle = eventData.EventTitle;
                DateTime eventDate = eventData.EventDate;

                var emailContent = $@"
                    Subject: Event Cancellation Notice - {eventTitle}
                    
                    Dear Customer,
                    
                    We regret to inform you that the following event has been cancelled:
                    
                    Event: {eventTitle}
                    Originally scheduled for: {eventDate:MMMM d, yyyy 'at' h:mm tt}
                    
                    You will receive a full refund for your tickets within 3-5 business days.
                    
                    We apologize for any inconvenience caused.
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Event cancellation notice for {EventTitle}", customerEmail, eventTitle);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event cancellation email");
            }
        }
        
        private async Task SendEventUpdateNotificationAsync(object data)
        {
            try
            {
                dynamic eventData = data;
                
                string customerEmail = eventData.CustomerEmail;
                string eventTitle = eventData.EventTitle;
                string updateMessage = eventData.UpdateMessage;

                var emailContent = $@"
                    Subject: Event Update - {eventTitle}
                    
                    Dear Customer,
                    
                    There has been an update to your event:
                    
                    Event: {eventTitle}
                    Update: {updateMessage}
                    
                    Please check your account for the latest details.
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Event update for {EventTitle}", customerEmail, eventTitle);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event update email");
            }
        }

        private async Task SendEventReminderAsync(object data)
        {
            try
            {
                dynamic reminderData = data;
                
                string customerEmail = reminderData.CustomerEmail;
                string eventTitle = reminderData.EventTitle;
                DateTime eventDate = reminderData.EventDate;
                string ticketNumber = reminderData.TicketNumber;

                var emailContent = $@"
                    Subject: Event Reminder - {eventTitle}
                    
                    Dear Customer,
                    
                    This is a friendly reminder about your upcoming event:
                    
                    Event: {eventTitle}
                    Date: {eventDate:MMMM d, yyyy 'at' h:mm tt}
                    Your Ticket: {ticketNumber}
                    
                    We look forward to seeing you there!
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Event reminder for {EventTitle}", customerEmail, eventTitle);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event reminder email");
            }
        }

        private async Task SendRefundNotificationAsync(object data)
        {
            try
            {
                dynamic refundData = data;
                
                string customerEmail = refundData.CustomerEmail;
                string ticketNumber = refundData.TicketNumber;
                decimal refundAmount = refundData.RefundAmount;

                var emailContent = $@"
                    Subject: Refund Processed - Ticket {ticketNumber}
                    
                    Dear Customer,
                    
                    Your refund has been processed:
                    
                    Ticket Number: {ticketNumber}
                    Refund Amount: ${refundAmount:F2}
                    
                    The refund will appear in your account within 3-5 business days.
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Refund notification for ticket {TicketNumber}", customerEmail, ticketNumber);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification email");
            }
        }

        private async Task SendTicketCancellationNotificationAsync(object data)
        {
            try
            {
                dynamic ticketData = data;
                
                string customerEmail = ticketData.CustomerEmail;
                string ticketNumber = ticketData.TicketNumber;
                string eventTitle = ticketData.EventTitle;
                decimal amount = ticketData.Amount;

                var emailContent = $@"
                    Subject: Ticket Cancellation Confirmation - {ticketNumber}
                    
                    Dear Customer,
                    
                    Your ticket cancellation has been processed:
                    
                    Event: {eventTitle}
                    Ticket Number: {ticketNumber}
                    Refund Amount: ${amount:F2}
                    
                    Your refund will be processed within 3-5 business days.
                    
                    Best regards,
                    Ticksy Team
                ";

                _logger.LogInformation("EMAIL SENT to {Email}: Ticket cancellation confirmation for {TicketNumber}", customerEmail, ticketNumber);
                _logger.LogInformation("Email Content: {Content}", emailContent);
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket cancellation email");
            }
        }
    }
} 