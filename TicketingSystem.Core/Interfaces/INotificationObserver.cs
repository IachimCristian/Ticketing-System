using System.Threading.Tasks;

namespace TicketingSystem.Core.Interfaces
{
    public interface INotificationObserver
    {
        Task UpdateAsync(string eventType, object data);
    }
} 