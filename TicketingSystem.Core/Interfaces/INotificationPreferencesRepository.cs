using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface INotificationPreferencesRepository : IRepository<NotificationPreferences>
    {
        Task<NotificationPreferences> GetByCustomerIdAsync(Guid customerId);
        Task<NotificationPreferences> CreateDefaultPreferencesAsync(Guid customerId);
        Task UpdatePreferencesAsync(NotificationPreferences preferences);
    }
} 