using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class NotificationPreferencesRepository : Repository<NotificationPreferences>, INotificationPreferencesRepository
    {
        private readonly ILogger<NotificationPreferencesRepository> _logger;

        public NotificationPreferencesRepository(AppDbContext context, ILogger<NotificationPreferencesRepository> logger)
            : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<NotificationPreferences> GetByCustomerIdAsync(Guid customerId)
        {
            try
            {
                var preferences = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);

                // If no preferences exist, create default ones
                if (preferences == null)
                {
                    preferences = await CreateDefaultPreferencesAsync(customerId);
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<NotificationPreferences> CreateDefaultPreferencesAsync(Guid customerId)
        {
            try
            {
                var preferences = new NotificationPreferences
                {
                    CustomerId = customerId
                    // Default values are set in the entity constructor
                };

                await _dbSet.AddAsync(preferences);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created default notification preferences for customer {CustomerId}", customerId);
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default notification preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task UpdatePreferencesAsync(NotificationPreferences preferences)
        {
            try
            {
                preferences.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(preferences);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated notification preferences for customer {CustomerId}", preferences.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for customer {CustomerId}", preferences.CustomerId);
                throw;
            }
        }
    }
} 