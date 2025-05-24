using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class NotificationService : INotificationSubject
    {
        private readonly List<INotificationObserver> _observers;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _observers = new List<INotificationObserver>();
            _logger = logger;
        }

        public async Task RegisterObserverAsync(INotificationObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                _logger.LogInformation($"Observer of type {observer.GetType().Name} registered");
                await Task.CompletedTask;
            }
        }

        public async Task UnregisterObserverAsync(INotificationObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
                _logger.LogInformation($"Observer of type {observer.GetType().Name} unregistered");
                await Task.CompletedTask;
            }
        }

        public async Task NotifyObserversAsync(string eventType, object data)
        {
            _logger.LogInformation($"Notifying observers about event: {eventType}");
            
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.UpdateAsync(eventType, data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error notifying observer {observer.GetType().Name}");
                }
            }
        }

        public async Task NotifyAsync(object notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }
            
            // Extract event type from the dynamic notification object
            string eventType = "Unknown";
            
            // Try to extract Type property
            try
            {
                dynamic dynamicNotification = notification;
                eventType = dynamicNotification.Type;
            }
            catch
            {
                _logger.LogWarning("Could not extract Type property from notification object");
            }
            
            await NotifyObserversAsync(eventType, notification);
        }
    }
} 