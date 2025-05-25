using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class CustomerNotificationService : ICustomerNotificationService
    {
        private readonly ICustomerNotificationRepository _notificationRepository;
        private readonly INotificationPreferencesRepository _preferencesRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;
        private readonly INotificationSubject _notificationSubject;
        private readonly ILogger<CustomerNotificationService> _logger;

        public CustomerNotificationService(
            ICustomerNotificationRepository notificationRepository,
            INotificationPreferencesRepository preferencesRepository,
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            INotificationSubject notificationSubject,
            ILogger<CustomerNotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _preferencesRepository = preferencesRepository;
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _notificationSubject = notificationSubject;
            _logger = logger;
        }

        public async Task<CustomerNotification> CreateNotificationAsync(
            Guid customerId, 
            string title, 
            string message, 
            string type, 
            string priority = "Medium",
            Guid? eventId = null,
            Guid? ticketId = null,
            string actionUrl = null,
            string actionText = null)
        {
            try
            {
                var notification = new CustomerNotification
                {
                    CustomerId = customerId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = priority,
                    EventId = eventId,
                    TicketId = ticketId,
                    ActionUrl = actionUrl,
                    ActionText = actionText
                };

                await _notificationRepository.AddAsync(notification);
                await _notificationRepository.SaveChangesAsync();

                _logger.LogInformation("Created notification {NotificationId} for customer {CustomerId}", 
                    notification.Id, customerId);

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerNotification>> GetCustomerNotificationsAsync(Guid customerId)
        {
            return await _notificationRepository.GetNotificationsByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<CustomerNotification>> GetUnreadNotificationsAsync(Guid customerId)
        {
            return await _notificationRepository.GetUnreadNotificationsByCustomerAsync(customerId);
        }

        public async Task<int> GetUnreadCountAsync(Guid customerId)
        {
            return await _notificationRepository.GetUnreadCountByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<CustomerNotification>> GetRecentNotificationsAsync(Guid customerId, int count = 10)
        {
            return await _notificationRepository.GetRecentNotificationsAsync(customerId, count);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadAsync(Guid customerId)
        {
            await _notificationRepository.MarkAllAsReadAsync(customerId);
        }

        public async Task<NotificationPreferences> GetNotificationPreferencesAsync(Guid customerId)
        {
            return await _preferencesRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task UpdateNotificationPreferencesAsync(NotificationPreferences preferences)
        {
            await _preferencesRepository.UpdatePreferencesAsync(preferences);
        }

        public async Task SendTicketPurchaseNotificationAsync(Guid customerId, Ticket ticket, Event eventDetails)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(customerId);

                // Create in-app notification if enabled
                if (preferences.InAppTicketPurchase)
                {
                    await CreateNotificationAsync(
                        customerId,
                        "Ticket Purchase Confirmed",
                        $"Your ticket for '{eventDetails.Title}' has been successfully purchased. Ticket number: {ticket.TicketNumber}",
                        "TicketPurchase",
                        "High",
                        eventDetails.Id,
                        ticket.Id,
                        $"/Tickets/Confirmation/{ticket.Id}",
                        "View Ticket"
                    );
                }

                // Send external notifications based on preferences
                var notificationData = new
                {
                    CustomerId = customerId,
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    EventTitle = eventDetails.Title,
                    EventDate = eventDetails.StartDate,
                    Amount = ticket.Price,
                    SendEmail = preferences.EmailTicketPurchase,
                    SendSms = preferences.SmsTicketPurchase
                };

                await _notificationSubject.NotifyObserversAsync("TicketPurchased", notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket purchase notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendEventUpdateNotificationAsync(Guid customerId, Event eventDetails, string updateMessage)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(customerId);

                if (preferences.InAppEventUpdates)
                {
                    await CreateNotificationAsync(
                        customerId,
                        $"Event Update: {eventDetails.Title}",
                        updateMessage,
                        "EventUpdate",
                        "Medium",
                        eventDetails.Id,
                        null,
                        $"/Events/Details/{eventDetails.Id}",
                        "View Event"
                    );
                }

                var notificationData = new
                {
                    CustomerId = customerId,
                    EventId = eventDetails.Id,
                    EventTitle = eventDetails.Title,
                    UpdateMessage = updateMessage,
                    SendEmail = preferences.EmailEventUpdates,
                    SendSms = preferences.SmsEventUpdates
                };

                await _notificationSubject.NotifyObserversAsync("EventUpdated", notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event update notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendEventCancellationNotificationAsync(Guid customerId, Event eventDetails)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(customerId);

                if (preferences.InAppEventCancellations)
                {
                    await CreateNotificationAsync(
                        customerId,
                        $"Event Cancelled: {eventDetails.Title}",
                        $"Unfortunately, the event '{eventDetails.Title}' scheduled for {eventDetails.StartDate:MMMM d, yyyy} has been cancelled. You will receive a full refund for your tickets.",
                        "EventCancellation",
                        "Urgent",
                        eventDetails.Id,
                        null,
                        $"/Events/Details/{eventDetails.Id}",
                        "View Details"
                    );
                }

                var notificationData = new
                {
                    CustomerId = customerId,
                    EventId = eventDetails.Id,
                    EventTitle = eventDetails.Title,
                    EventDate = eventDetails.StartDate,
                    SendEmail = preferences.EmailEventCancellations,
                    SendSms = preferences.SmsEventCancellations
                };

                await _notificationSubject.NotifyObserversAsync("EventCancelled", notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event cancellation notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendEventReminderNotificationAsync(Guid customerId, Event eventDetails, Ticket ticket)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(customerId);

                if (preferences.InAppEventReminders)
                {
                    var hoursUntilEvent = (eventDetails.StartDate - DateTime.UtcNow).TotalHours;
                    var timeText = hoursUntilEvent > 24 ? $"{Math.Round(hoursUntilEvent / 24)} days" : $"{Math.Round(hoursUntilEvent)} hours";

                    await CreateNotificationAsync(
                        customerId,
                        $"Event Reminder: {eventDetails.Title}",
                        $"Don't forget! Your event '{eventDetails.Title}' starts in {timeText} at {eventDetails.Location}. Ticket: {ticket.TicketNumber}",
                        "EventReminder",
                        "Medium",
                        eventDetails.Id,
                        ticket.Id,
                        $"/Tickets/Confirmation/{ticket.Id}",
                        "View Ticket"
                    );
                }

                var notificationData = new
                {
                    CustomerId = customerId,
                    EventId = eventDetails.Id,
                    EventTitle = eventDetails.Title,
                    EventDate = eventDetails.StartDate,
                    TicketNumber = ticket.TicketNumber,
                    SendEmail = preferences.EmailEventReminders,
                    SendSms = preferences.SmsEventReminders
                };

                await _notificationSubject.NotifyObserversAsync("EventReminder", notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event reminder notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendRefundNotificationAsync(Guid customerId, Ticket ticket, decimal refundAmount)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(customerId);

                if (preferences.InAppRefundUpdates)
                {
                    await CreateNotificationAsync(
                        customerId,
                        "Refund Processed",
                        $"Your refund of ${refundAmount:F2} for ticket {ticket.TicketNumber} has been processed and will appear in your account within 3-5 business days.",
                        "Refund",
                        "High",
                        ticket.EventId,
                        ticket.Id,
                        $"/Tickets/Confirmation/{ticket.Id}",
                        "View Details"
                    );
                }

                var notificationData = new
                {
                    CustomerId = customerId,
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    RefundAmount = refundAmount,
                    SendEmail = preferences.EmailRefundUpdates,
                    SendSms = preferences.SmsRefundUpdates
                };

                await _notificationSubject.NotifyObserversAsync("RefundProcessed", notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification for customer {CustomerId}", customerId);
            }
        }

        public async Task SendBulkEventUpdateNotificationAsync(Event eventDetails, string updateMessage)
        {
            try
            {
                // Get all customers who have tickets for this event
                var tickets = await _ticketRepository.GetTicketsByEventAsync(eventDetails.Id);
                var customerIds = tickets.Where(t => t.Status == "Sold").Select(t => t.CustomerId).Distinct();

                foreach (var customerId in customerIds)
                {
                    await SendEventUpdateNotificationAsync(customerId, eventDetails, updateMessage);
                }

                _logger.LogInformation("Sent bulk event update notifications for event {EventId} to {CustomerCount} customers", 
                    eventDetails.Id, customerIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk event update notifications for event {EventId}", eventDetails.Id);
            }
        }

        public async Task SendBulkEventCancellationNotificationAsync(Event eventDetails)
        {
            try
            {
                // Get all customers who have tickets for this event
                var tickets = await _ticketRepository.GetTicketsByEventAsync(eventDetails.Id);
                var customerIds = tickets.Where(t => t.Status == "Sold").Select(t => t.CustomerId).Distinct();

                foreach (var customerId in customerIds)
                {
                    await SendEventCancellationNotificationAsync(customerId, eventDetails);
                }

                _logger.LogInformation("Sent bulk event cancellation notifications for event {EventId} to {CustomerCount} customers", 
                    eventDetails.Id, customerIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk event cancellation notifications for event {EventId}", eventDetails.Id);
            }
        }

        public async Task SendEventRemindersAsync()
        {
            try
            {
                var upcomingEvents = await _eventRepository.GetUpcomingEventsAsync();
                var now = DateTime.UtcNow;

                foreach (var eventDetails in upcomingEvents)
                {
                    var hoursUntilEvent = (eventDetails.StartDate - now).TotalHours;
                    
                    // Send reminders for events starting in 24 hours (with some tolerance)
                    if (hoursUntilEvent > 23 && hoursUntilEvent <= 25)
                    {
                        var tickets = await _ticketRepository.GetTicketsByEventAsync(eventDetails.Id);
                        var soldTickets = tickets.Where(t => t.Status == "Sold");

                        foreach (var ticket in soldTickets)
                        {
                            var preferences = await GetNotificationPreferencesAsync(ticket.CustomerId);
                            
                            // Check if reminder should be sent based on customer's preference
                            if (Math.Abs(hoursUntilEvent - preferences.EventReminderHours) <= 1)
                            {
                                await SendEventReminderNotificationAsync(ticket.CustomerId, eventDetails, ticket);
                            }
                        }
                    }
                }

                _logger.LogInformation("Processed event reminders for {EventCount} upcoming events", upcomingEvents.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending event reminders");
            }
        }

        public async Task CleanupOldNotificationsAsync(int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                await _notificationRepository.DeleteOldNotificationsAsync(cutoffDate);
                
                _logger.LogInformation("Cleaned up notifications older than {DaysToKeep} days", daysToKeep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old notifications");
            }
        }
    }
} 