using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Observers
{
    public class SMSNotificationObserver : INotificationObserver
    {
        public async Task UpdateAsync(string eventType, object data)
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
                default:
                    Console.WriteLine($"Unsupported event type for SMS notification: {eventType}");
                    break;
            }
        }
        
        private Task SendTicketPurchaseConfirmationAsync(object data)
        {
            // In a real application, this would send an actual SMS
            Console.WriteLine("Sending ticket purchase confirmation SMS");
            return Task.CompletedTask;
        }
        
        private Task SendEventCancellationNotificationAsync(object data)
        {
            // In a real application, this would send an actual SMS
            Console.WriteLine("Sending event cancellation notification SMS");
            return Task.CompletedTask;
        }
        
        private Task SendEventReminderAsync(object data)
        {
            // In a real application, this would send an actual SMS
            Console.WriteLine("Sending event reminder SMS");
            return Task.CompletedTask;
        }
    }
} 