using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services.Observers
{
    public class EmailNotificationObserver : INotificationObserver
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
                case "EventUpdated":
                    await SendEventUpdateNotificationAsync(data);
                    break;
                default:
                    Console.WriteLine($"Unsupported event type for email notification: {eventType}");
                    break;
            }
        }
        
        private Task SendTicketPurchaseConfirmationAsync(object data)
        {
            // In a real application, this would send an actual email
            Console.WriteLine("Sending ticket purchase confirmation email");
            return Task.CompletedTask;
        }
        
        private Task SendEventCancellationNotificationAsync(object data)
        {
            // In a real application, this would send an actual email
            Console.WriteLine("Sending event cancellation notification email");
            return Task.CompletedTask;
        }
        
        private Task SendEventUpdateNotificationAsync(object data)
        {
            // In a real application, this would send an actual email
            Console.WriteLine("Sending event update notification email");
            return Task.CompletedTask;
        }
    }
} 