using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace TicketingSystem.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;

        public Repository(AppDbContext context, ILogger<Repository<T>> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
                return new List<T>();
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error finding entities of type {EntityType} with predicate", typeof(T).Name);
                return new List<T>();
            }
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                return null;
            }
        }

        public async Task AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public Task UpdateAsync(T entity)
        {
            try
            {
                // Check if entity is already being tracked
                var trackedEntity = _context.ChangeTracker.Entries<T>()
                    .FirstOrDefault(e => e.Entity.GetType() == entity.GetType() && 
                                       e.Property("Id").CurrentValue.Equals(
                                           entity.GetType().GetProperty("Id")?.GetValue(entity)));

                if (trackedEntity != null)
                {
                    // Detach the existing tracked entity
                    trackedEntity.State = EntityState.Detached;
                }

                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public Task DeleteAsync(T entity)
        {
            try
            {
                _dbSet.Remove(entity);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        _logger.LogDebug("Entity: {EntityType}, State: {State}", 
                            entry.Entity.GetType().Name, 
                            entry.State);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency error while saving changes for entity type {EntityType}", typeof(T).Name);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Database update error while saving changes for entity type {EntityType}", typeof(T).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes for entity type {EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
} 