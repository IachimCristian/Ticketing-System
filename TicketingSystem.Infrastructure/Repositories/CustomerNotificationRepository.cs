using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class CustomerNotificationRepository : Repository<CustomerNotification>, ICustomerNotificationRepository
    {
        private readonly ILogger<CustomerNotificationRepository> _logger;

        public CustomerNotificationRepository(AppDbContext context, ILogger<CustomerNotificationRepository> logger)
            : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerNotification>> GetNotificationsByCustomerAsync(Guid customerId)
        {
            try
            {
                return await _dbSet
                    .Include(n => n.Event)
                    .Include(n => n.Ticket)
                    .Where(n => n.CustomerId == customerId)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for customer {CustomerId}", customerId);
                return new List<CustomerNotification>();
            }
        }

        public async Task<IEnumerable<CustomerNotification>> GetUnreadNotificationsByCustomerAsync(Guid customerId)
        {
            try
            {
                return await _dbSet
                    .Include(n => n.Event)
                    .Include(n => n.Ticket)
                    .Where(n => n.CustomerId == customerId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications for customer {CustomerId}", customerId);
                return new List<CustomerNotification>();
            }
        }

        public async Task<int> GetUnreadCountByCustomerAsync(Guid customerId)
        {
            try
            {
                return await _dbSet
                    .CountAsync(n => n.CustomerId == customerId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting unread notifications for customer {CustomerId}", customerId);
                return 0;
            }
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _dbSet.FindAsync(notificationId);
                if (notification != null && !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(Guid customerId)
        {
            try
            {
                var unreadNotifications = await _dbSet
                    .Where(n => n.CustomerId == customerId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerNotification>> GetNotificationsByTypeAsync(Guid customerId, string type)
        {
            try
            {
                return await _dbSet
                    .Include(n => n.Event)
                    .Include(n => n.Ticket)
                    .Where(n => n.CustomerId == customerId && n.Type == type)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications of type {Type} for customer {CustomerId}", type, customerId);
                return new List<CustomerNotification>();
            }
        }

        public async Task<IEnumerable<CustomerNotification>> GetRecentNotificationsAsync(Guid customerId, int count = 10)
        {
            try
            {
                return await _dbSet
                    .Include(n => n.Event)
                    .Include(n => n.Ticket)
                    .Where(n => n.CustomerId == customerId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(count)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent notifications for customer {CustomerId}", customerId);
                return new List<CustomerNotification>();
            }
        }

        public async Task DeleteOldNotificationsAsync(DateTime cutoffDate)
        {
            try
            {
                var oldNotifications = await _dbSet
                    .Where(n => n.CreatedAt < cutoffDate)
                    .ToListAsync();

                _dbSet.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} old notifications created before {CutoffDate}", 
                    oldNotifications.Count, cutoffDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting old notifications created before {CutoffDate}", cutoffDate);
                throw;
            }
        }
    }
} 