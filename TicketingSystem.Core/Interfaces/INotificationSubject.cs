using System.Threading.Tasks;

namespace TicketingSystem.Core.Interfaces
{
    public interface INotificationSubject
    {
        Task RegisterObserverAsync(INotificationObserver observer);
        Task UnregisterObserverAsync(INotificationObserver observer);
        Task NotifyObserversAsync(string eventType, object data);
        Task NotifyAsync(object notification);
    }
} 