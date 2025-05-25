using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces
{
    public interface ICustomerNotificationRepository : IRepository<CustomerNotification>
    {
        Task<IEnumerable<CustomerNotification>> GetNotificationsByCustomerAsync(Guid customerId);
        Task<IEnumerable<CustomerNotification>> GetUnreadNotificationsByCustomerAsync(Guid customerId);
        Task<int> GetUnreadCountByCustomerAsync(Guid customerId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid customerId);
        Task<IEnumerable<CustomerNotification>> GetNotificationsByTypeAsync(Guid customerId, string type);
        Task<IEnumerable<CustomerNotification>> GetRecentNotificationsAsync(Guid customerId, int count = 10);
        Task DeleteOldNotificationsAsync(DateTime cutoffDate);
    }
} 