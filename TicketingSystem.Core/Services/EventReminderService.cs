using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class EventReminderService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly ICustomerNotificationService _customerNotificationService;
        private readonly INotificationSubject _notificationSubject;
        private readonly ILogger<EventReminderService> _logger;

        public EventReminderService(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            IUserRepository<Customer> customerRepository,
            ICustomerNotificationService customerNotificationService,
            INotificationSubject notificationSubject,
            ILogger<EventReminderService> logger)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _customerRepository = customerRepository;
            _customerNotificationService = customerNotificationService;
            _notificationSubject = notificationSubject;
            _logger = logger;
        }

        public async Task SendEventRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting event reminder check...");

                // Get all active upcoming events
                var allUpcomingEvents = await _eventRepository.GetUpcomingEventsAsync();
                _logger.LogInformation("Found {EventCount} upcoming events", allUpcomingEvents.Count());
                
                // Filter to events happening in the next 7 days
                var upcomingEvents = allUpcomingEvents.Where(e => 
                    e.StartDate > DateTime.UtcNow && 
                    e.StartDate <= DateTime.UtcNow.AddDays(7) &&
                    e.IsActive).ToList();
                
                _logger.LogInformation("Filtered to {FilteredEventCount} events happening in next 7 days", upcomingEvents.Count);

                if (!upcomingEvents.Any())
                {
                    _logger.LogInformation("No upcoming events found in the next 7 days");
                    return;
                }

                foreach (var eventItem in upcomingEvents)
                {
                    _logger.LogInformation("Processing event: {EventTitle} on {EventDate}", eventItem.Title, eventItem.StartDate);
                    await ProcessEventRemindersAsync(eventItem);
                }

                _logger.LogInformation("Event reminder check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event reminders");
            }
        }

        private async Task ProcessEventRemindersAsync(Event eventItem)
        {
            try
            {
                // Get all sold tickets for this event
                var tickets = await _ticketRepository.GetTicketsByEventAsync(eventItem.Id);
                var soldTickets = tickets.Where(t => t.Status == "Sold").ToList();

                _logger.LogInformation("Event {EventTitle}: Found {TotalTickets} tickets, {SoldTickets} sold", 
                    eventItem.Title, tickets.Count(), soldTickets.Count);

                if (!soldTickets.Any())
                {
                    _logger.LogInformation("No sold tickets found for event {EventTitle}", eventItem.Title);
                    return;
                }

                foreach (var ticket in soldTickets)
                {
                    await ProcessTicketReminderAsync(ticket, eventItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminders for event {EventId}", eventItem.Id);
            }
        }

        private async Task ProcessTicketReminderAsync(Ticket ticket, Event eventItem)
        {
            try
            {
                // Get customer and their notification preferences
                var customer = await _customerRepository.GetByIdAsync(ticket.CustomerId);
                if (customer == null) 
                {
                    _logger.LogWarning("Customer not found for ticket {TicketId}", ticket.Id);
                    return;
                }

                var preferences = await _customerNotificationService.GetNotificationPreferencesAsync(ticket.CustomerId);
                
                // Calculate hours until event
                var hoursUntilEvent = (eventItem.StartDate - DateTime.UtcNow).TotalHours;
                
                _logger.LogInformation("Processing ticket {TicketNumber} for customer {CustomerEmail}: Event in {Hours} hours", 
                    ticket.TicketNumber, customer.Email, Math.Round(hoursUntilEvent, 2));
                
                // Check if we should send a reminder based on customer's preference
                if (ShouldSendReminder(hoursUntilEvent, preferences.EventReminderHours))
                {
                    // Check if we haven't already sent a reminder for this timeframe
                    if (!await HasRecentReminderBeenSent(ticket.CustomerId, eventItem.Id, preferences.EventReminderHours))
                    {
                        _logger.LogInformation("Sending reminder for ticket {TicketNumber}", ticket.TicketNumber);
                        await SendReminderNotificationAsync(ticket, eventItem, customer, preferences);
                    }
                    else
                    {
                        _logger.LogInformation("Recent reminder already sent for ticket {TicketNumber}", ticket.TicketNumber);
                    }
                }
                else
                {
                    _logger.LogInformation("Reminder not needed for ticket {TicketNumber} at this time", ticket.TicketNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder for ticket {TicketId}", ticket.Id);
            }
        }

        private bool ShouldSendReminder(double hoursUntilEvent, int reminderHours)
        {
            // For testing purposes, send reminders for any event within 7 days (168 hours)
            // In production, you might want to be more strict about the timing
            if (hoursUntilEvent <= 168 && hoursUntilEvent > 0) // Within 7 days and in the future
            {
                _logger.LogInformation("Should send reminder: Event in {Hours} hours, customer preference: {ReminderHours} hours", 
                    hoursUntilEvent, reminderHours);
                return true;
            }
            
            _logger.LogInformation("Should NOT send reminder: Event in {Hours} hours, customer preference: {ReminderHours} hours", 
                hoursUntilEvent, reminderHours);
            return false;
        }

        private async Task<bool> HasRecentReminderBeenSent(Guid customerId, Guid eventId, int reminderHours)
        {
            try
            {
                // Check if a reminder notification was sent in the last 2 hours for this event
                var recentNotifications = await _customerNotificationService.GetRecentNotificationsAsync(customerId, 50);
                
                var recentReminder = recentNotifications.FirstOrDefault(n => 
                    n.Type == "EventReminder" && 
                    n.EventId == eventId && 
                    n.CreatedAt >= DateTime.UtcNow.AddHours(-2));

                return recentReminder != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for recent reminders");
                return false; // If we can't check, err on the side of sending the reminder
            }
        }

        private async Task SendReminderNotificationAsync(Ticket ticket, Event eventItem, Customer customer, NotificationPreferences preferences)
        {
            try
            {
                _logger.LogInformation("Sending event reminder for ticket {TicketId}, event {EventTitle}", ticket.Id, eventItem.Title);

                // Send through the customer notification service (handles in-app notifications based on preferences)
                await _customerNotificationService.SendEventReminderNotificationAsync(ticket.CustomerId, eventItem, ticket);

                // Send through the observer pattern for email/SMS
                await _notificationSubject.NotifyObserversAsync("EventReminder", new
                {
                    CustomerId = ticket.CustomerId,
                    CustomerEmail = customer.Email,
                    CustomerPhone = customer.Phone,
                    EventId = eventItem.Id,
                    EventTitle = eventItem.Title,
                    EventDate = eventItem.StartDate,
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    SendEmail = preferences.EmailEventReminders,
                    SendSms = preferences.SmsEventReminders
                });

                _logger.LogInformation("Event reminder sent successfully for ticket {TicketId}", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder notification for ticket {TicketId}", ticket.Id);
            }
        }

        public async Task SendTestReminderAsync(Guid customerId, Guid eventId)
        {
            try
            {
                _logger.LogInformation("Sending test reminder for customer {CustomerId}, event {EventId}", customerId, eventId);

                var customer = await _customerRepository.GetByIdAsync(customerId);
                var eventItem = await _eventRepository.GetByIdAsync(eventId);
                
                if (customer == null || eventItem == null)
                {
                    _logger.LogWarning("Customer or event not found for test reminder");
                    return;
                }

                // Get customer's tickets for this event
                var tickets = await _ticketRepository.GetTicketsByEventAsync(eventId);
                var customerTicket = tickets.FirstOrDefault(t => t.CustomerId == customerId && t.Status == "Sold");
                
                if (customerTicket == null)
                {
                    _logger.LogWarning("No sold ticket found for customer {CustomerId} and event {EventId}", customerId, eventId);
                    return;
                }

                var preferences = await _customerNotificationService.GetNotificationPreferencesAsync(customerId);
                await SendReminderNotificationAsync(customerTicket, eventItem, customer, preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test reminder");
            }
        }
    }
} 