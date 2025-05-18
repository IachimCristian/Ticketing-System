using System.Threading.Tasks;

namespace TicketingSystem.Core.Interfaces
{
    public interface INotificationSubject
    {
        Task RegisterObserverAsync(INotificationObserver observer);
        Task RemoveObserverAsync(INotificationObserver observer);
        Task NotifyObserversAsync(string eventType, object data);
    }
} 