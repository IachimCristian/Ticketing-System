using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public class NotificationService : INotificationSubject
    {
        private readonly List<INotificationObserver> _observers = new List<INotificationObserver>();
        
        public Task RegisterObserverAsync(INotificationObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return Task.CompletedTask;
        }
        
        public Task RemoveObserverAsync(INotificationObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
            return Task.CompletedTask;
        }
        
        public async Task NotifyObserversAsync(string eventType, object data)
        {
            foreach (var observer in _observers)
            {
                await observer.UpdateAsync(eventType, data);
            }
        }
    }
} 