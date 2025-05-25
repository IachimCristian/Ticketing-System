using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ICustomerNotificationService
    {
        // Create notifications
        Task<CustomerNotification> CreateNotificationAsync(
            Guid customerId, 
            string title, 
            string message, 
            string type, 
            string priority = "Medium",
            Guid? eventId = null,
            Guid? ticketId = null,
            string actionUrl = null,
            string actionText = null);

        // Get notifications
        Task<IEnumerable<CustomerNotification>> GetCustomerNotificationsAsync(Guid customerId);
        Task<IEnumerable<CustomerNotification>> GetUnreadNotificationsAsync(Guid customerId);
        Task<int> GetUnreadCountAsync(Guid customerId);
        Task<IEnumerable<CustomerNotification>> GetRecentNotificationsAsync(Guid customerId, int count = 10);

        // Mark as read
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid customerId);

        // Notification preferences
        Task<NotificationPreferences> GetNotificationPreferencesAsync(Guid customerId);
        Task UpdateNotificationPreferencesAsync(NotificationPreferences preferences);

        // Send notifications based on preferences
        Task SendTicketPurchaseNotificationAsync(Guid customerId, Ticket ticket, Event eventDetails);
        Task SendEventUpdateNotificationAsync(Guid customerId, Event eventDetails, string updateMessage);
        Task SendEventCancellationNotificationAsync(Guid customerId, Event eventDetails);
        Task SendEventReminderNotificationAsync(Guid customerId, Event eventDetails, Ticket ticket);
        Task SendRefundNotificationAsync(Guid customerId, Ticket ticket, decimal refundAmount);

        // Bulk operations
        Task SendBulkEventUpdateNotificationAsync(Event eventDetails, string updateMessage);
        Task SendBulkEventCancellationNotificationAsync(Event eventDetails);
        Task SendEventRemindersAsync(); // Send reminders for upcoming events

        // Cleanup
        Task CleanupOldNotificationsAsync(int daysToKeep = 90);
    }
} 